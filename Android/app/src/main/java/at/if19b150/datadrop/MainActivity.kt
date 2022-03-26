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
    var fileData = mutableListOf<ByteArray?>()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val ipAddress = findViewById<EditText>(R.id.ipAdress)
        val port = findViewById<EditText>(R.id.port)
        val isSending = findViewById<RadioButton>(R.id.sending)
        val isReceiving = findViewById<RadioButton>(R.id.receive)
        val receiveButton = findViewById<Button>(R.id.receivingButton)
        val sendButton = findViewById<Button>(R.id.sendingButton)
        val qrScannPicture = findViewById<ImageView>(R.id.qrScanner)
        val qrScannPicture2 = findViewById<ImageView>(R.id.qrScanner2)
        val fileInformation: FileInformation = FileInformation("abc.txt", "txt", 100, 1)
        debugTextView = findViewById<TextView>(R.id.debugTextView)

        isReceiving.setOnClickListener{
            sendButton.isVisible = false
            receiveButton.isVisible = true
            qrScannPicture.isVisible = true
            qrScannPicture2.isVisible = true
        }

        isSending.setOnClickListener{
            sendButton.isVisible = true
            receiveButton.isVisible = false
            qrScannPicture.isVisible = false
            qrScannPicture2.isVisible = false

            fileData.add("1.Line \n".toByteArray())
            fileData.add("2.Line \n".toByteArray())
            fileData.add("3.Line \n".toByteArray())
            fileData.add("4.Line \n".toByteArray())

            createFile(fileInformation)
        }

        receiveButton.setOnClickListener {
            GlobalScope.launch(IO) {
                startDownload(ipAddress.text.toString(), port.text.toString().trim().toInt())
            }
        }
    }
    override fun onActivityResult(requestCode: Int, resultCode: Int, resultData: Intent?) {
        if (requestCode == 49153 && resultCode == Activity.RESULT_OK) {
            // The result data contains a URI for the document or directory that
            // the user selected.
            resultData?.data?.also { uri ->
                alterDocument(uri)
            }
        }
    }

    public suspend fun startDownload(ipAddress: String, port: Int) {
        var fileInformation = getFileInformation(ipAddress, port)

        createFile(fileInformation)

        for (i in 0..fileInformation.SequenzeCount) {
            var tempData = getResponseFromSocket(ipAddress, port, AllowedPaths.SendData, i.toString())
            fileData.add(tempData)
            println("${Calendar.getInstance().getTime()} Received Packages: $i")
        }
    }

    private fun alterDocument(uri: Uri){
        try {
                contentResolver.openFileDescriptor(uri, "w")?.use {
                    FileOutputStream(it.fileDescriptor).use {
                        fileData.forEach { data ->
                            it.write(data)
                        }
                    }
            }
        } catch (e: FileNotFoundException) {
            e.printStackTrace()
        } catch (e: IOException) {
            e.printStackTrace()
        }
    }

    private fun createFile(fileInformation: FileInformation) {
        val intent = Intent(Intent.ACTION_CREATE_DOCUMENT).apply {
            addCategory(Intent.CATEGORY_OPENABLE)
            type = "application/${fileInformation.FileExtension}"
            putExtra(Intent.EXTRA_TITLE, fileInformation.Filename)
        }
        startActivityForResult(intent, 49153)
    }

    public suspend fun getFileInformation(ipAddress: String, port: Int): FileInformation {
        var fileInformation : FileInformation? = null

        var fileInformationJsonData : String? = getResponseFromSocket(ipAddress, port, AllowedPaths.DataInformation)?.decodeToString()
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

    private suspend fun getResponseFromSocket(ipAddress : String, port : Int, path : AllowedPaths, resource : String = "") : ByteArray? {
        try {
            val response = withContext(IO) {
                // From Here https://ktor.io/docs/servers-raw-sockets.html#client
                val exec = Executors.newCachedThreadPool()
                val selector = ActorSelectorManager(exec.asCoroutineDispatcher())
                val socket = aSocket(selector).tcp().connect(InetSocketAddress(ipAddress, port))
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




