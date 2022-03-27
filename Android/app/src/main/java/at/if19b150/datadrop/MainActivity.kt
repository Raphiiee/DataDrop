package at.if19b150.datadrop

import android.app.Activity
import android.content.Intent
import android.icu.util.Calendar
import android.net.Uri
import android.os.Bundle
import android.os.SystemClock.sleep
import android.util.Log
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.isVisible
import io.ktor.network.selector.*
import io.ktor.network.sockets.*
import io.ktor.utils.io.*
import kotlinx.coroutines.*
import kotlinx.coroutines.Dispatchers.Default
import kotlinx.coroutines.Dispatchers.IO
import kotlinx.coroutines.Dispatchers.Main
import org.json.JSONObject
import java.io.*
import java.net.InetSocketAddress
import java.util.concurrent.Executors


class MainActivity : AppCompatActivity() {
    var debugTextView : TextView? = null
    var receiveButton : Button? = null
    var ipAddress : EditText? = null
    var port : EditText? = null
    var fileInformation = FileInformation("", "", 1, 1)
    var serverInformation = ServerInformation("", 0)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        ipAddress = findViewById<EditText>(R.id.ipAdress)
        port = findViewById<EditText>(R.id.port)
        val isSending = findViewById<RadioButton>(R.id.sending)
        val isReceiving = findViewById<RadioButton>(R.id.receive)
        val sendButton = findViewById<Button>(R.id.sendingButton)
        val qrScannPicture = findViewById<ImageView>(R.id.qrScanner)
        val qrScannPicture2 = findViewById<ImageView>(R.id.qrScanner2)
        debugTextView = findViewById<TextView>(R.id.debugTextView)
        receiveButton = findViewById<Button>(R.id.receivingButton)

        isReceiving.setOnClickListener{
            sendButton.isVisible = false
            receiveButton?.isVisible = true
            qrScannPicture.isVisible = true
            qrScannPicture2.isVisible = true
        }

        isSending.setOnClickListener{
            sendButton.isVisible = true
            receiveButton?.isVisible = false
            qrScannPicture.isVisible = false
            qrScannPicture2.isVisible = false
        }

        receiveButton?.setOnClickListener {
            GlobalScope.launch(Main) {
                receiveButton?.isEnabled = false;
                serverInformation = ServerInformation(ipAddress?.text.toString(), port?.text.toString().trim().toInt())
                createFile()
            }
        }

        qrScannPicture.setOnClickListener {
            val intent = Intent(this, QRCodeActivity::class.java)
            startActivityForResult(intent, 15)
        }

        qrScannPicture2.setOnClickListener {
            val intent = Intent(this, QRCodeActivity::class.java)
            startActivityForResult(intent, 15)
        }
    }
    override fun onActivityResult(requestCode: Int, resultCode: Int, resultData: Intent?) {
        if (requestCode == 49153 && resultCode == Activity.RESULT_OK) {
            // The result data contains a URI for the document or directory that
            // the user selected.
            resultData?.data?.also { uri ->
                GlobalScope.launch {
                    startDownload(serverInformation, uri)
                }
            }
        }
        if (requestCode == 15 && resultCode == Activity.RESULT_OK) {
            var jsonHostData = resultData?.extras?.getString("result") ?: ""
            val jsonRoot = JSONObject(jsonHostData)
            var hostInformation = HostInformation(jsonRoot.optString("IpAddress"),
                                                  jsonRoot.getInt("Port"),
                                                  jsonRoot.optBoolean("IsSsidOn"),
                                                  jsonRoot.optString("SsidName"),
                                                  jsonRoot.optString("SsidPassword"),
                                                  jsonRoot.optString("SsidNetworkIpAddress"),)
            ipAddress?.setText(hostInformation.IpAddress)
            port?.setText(hostInformation.Port.toString())
        }

    }

    public suspend fun startDownload(serverInformation: ServerInformation, uri: Uri) {

        for (i in 0..fileInformation.SequenzeCount) {
            var tempData = getResponseFromSocket(serverInformation, AllowedPaths.SendData, i.toString())
            fileInformation.FileData.add(tempData)
            if(i%10 == 0) {
                println("${Calendar.getInstance().getTime()} Received Packages: $i")
            }
        }

        alterDocument(uri)
    }

    private fun alterDocument(uri: Uri){
        try {
            contentResolver.openFileDescriptor(uri, "w")?.use {
                FileOutputStream(it.fileDescriptor).use {
                    fileInformation.FileData.forEach { data ->
                        it.write(data)
                    }
                }
            }
        } catch (e: FileNotFoundException) {
            e.printStackTrace()
        } catch (e: IOException) {
            e.printStackTrace()
        } finally {
            GlobalScope.launch(Main) {
                receiveButton?.isEnabled = true;
            }
        }
    }

    private suspend fun createFile() {
        fileInformation = getFileInformation(serverInformation)
        val intent = Intent(Intent.ACTION_CREATE_DOCUMENT).apply {
            addCategory(Intent.CATEGORY_OPENABLE)
            type = "application/${fileInformation.FileExtension}"
            putExtra(Intent.EXTRA_TITLE, fileInformation.Filename)
        }
        startActivityForResult(intent, 49153)
    }

    public suspend fun getFileInformation(serverInformation: ServerInformation): FileInformation {
        var fileInformation : FileInformation? = null

        var fileInformationJsonData : String? = getResponseFromSocket(serverInformation, AllowedPaths.DataInformation)?.decodeToString()
        var temp = ""
        fileInformationJsonData?.forEach {
            if (it != '\u0000') {
                temp += it
            }
        }

        fileInformation = withContext(Dispatchers.Default) {
            parseFileInformationResponse(temp)
        }

        return fileInformation
    }

    private fun parseFileInformationResponse(response : String): FileInformation {
        val jsonRoot = JSONObject(response)
        return FileInformation(jsonRoot.optString("Filename"), jsonRoot.optString("FileExtension"), jsonRoot.getInt("FileSize"), jsonRoot.getInt("BufferSize"))
    }

    private suspend fun getResponseFromSocket(serverInformation: ServerInformation, path : AllowedPaths, resource : String = "") : ByteArray? {
        try {
            val response = withContext(IO) {
                // From Here https://ktor.io/docs/servers-raw-sockets.html#client
                val exec = Executors.newCachedThreadPool()
                val selector = ActorSelectorManager(exec.asCoroutineDispatcher())
                val socket = aSocket(selector).tcp().connect(InetSocketAddress(serverInformation.ipAddress, serverInformation.port))
                val input : ByteReadChannel  = socket.openReadChannel()
                val output: ByteWriteChannel = socket.openWriteChannel(autoFlush = true)

                output.writeFully("Get /$path/$resource/".toByteArray())
                var responseArray = ByteArray(10000)
                input.readAvailable(responseArray)
                //println("Server said: '${String(responseArray)}'")

                socket.close()

                return@withContext responseArray
            }

            return response


        } catch (ex : IOException) {
            Log.e("LOG_TAG", "IO Exception: ", ex)
            return null
        } catch (ex : Exception) {
            Log.e("LOG_TAG", "Exception", ex)
            return null
        }
    }
}




