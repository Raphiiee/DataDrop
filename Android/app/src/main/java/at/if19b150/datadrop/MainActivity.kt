package at.if19b150.datadrop

import android.Manifest
import android.accounts.NetworkErrorException
import android.app.Activity
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.content.pm.PackageManager
import android.net.Uri
import android.net.wifi.WpsInfo
import android.net.wifi.p2p.WifiP2pConfig
import android.net.wifi.p2p.WifiP2pDevice
import android.net.wifi.p2p.WifiP2pManager
import android.os.Bundle
import android.util.Log
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.view.isVisible
import io.ktor.network.selector.*
import io.ktor.network.sockets.*
import io.ktor.utils.io.*
import kotlinx.coroutines.*
import kotlinx.coroutines.Dispatchers.IO
import kotlinx.coroutines.Dispatchers.Main
import org.json.JSONObject
import java.io.*
import java.net.ConnectException
import java.net.InetSocketAddress
import java.security.MessageDigest
import java.util.concurrent.Executors


class MainActivity : AppCompatActivity() {
    var debugTextView : TextView? = null
    var receiveButton : Button? = null
    var ipAddress : EditText? = null
    var port : EditText? = null
    var fileInformation = FileInformation("", "", 0, 0, 0, 0)
    var hashInformation = HashInformation(mutableMapOf<Int, String>())
    var hashComputed = mutableMapOf<Int, String>()
    var downloadAgain = mutableListOf<Int>()
    var serverInformation = ServerInformation("", 0)
    var failedDownloadedPackage : Int = 0
    var resumeUri : Uri? = null

    val manager: WifiP2pManager? by lazy(LazyThreadSafetyMode.NONE) {
        getSystemService(Context.WIFI_P2P_SERVICE) as WifiP2pManager?
    }
    var channel: WifiP2pManager.Channel? = null
    var receiver: BroadcastReceiver? = null
    val intentFilter = IntentFilter().apply {
        addAction(WifiP2pManager.WIFI_P2P_STATE_CHANGED_ACTION)
        addAction(WifiP2pManager.WIFI_P2P_PEERS_CHANGED_ACTION)
        addAction(WifiP2pManager.WIFI_P2P_CONNECTION_CHANGED_ACTION)
        addAction(WifiP2pManager.WIFI_P2P_THIS_DEVICE_CHANGED_ACTION)
    }
    private val peers = mutableListOf<WifiP2pDevice>()
    var hostInformation: HostInformation? = null

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
        val wifiDirectButton = findViewById<Button>(R.id.wifiDirectButton)
        receiveButton = findViewById<Button>(R.id.receivingButton)
        channel = manager?.initialize(this, mainLooper, null)
        channel?.also { channel ->
            receiver = WiFiDirectBroadcastReceiver(manager, channel, this)
        }
        discover()

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
                if (failedDownloadedPackage > 0 && resumeUri != null) {
                    startResumeDownload(serverInformation, resumeUri!!)
                } else {
                    receiveButton?.isEnabled = false;
                    serverInformation = ServerInformation(ipAddress?.text.toString(), port?.text.toString().trim().toInt())
                    createFile()
                }
            }
        }

        wifiDirectButton.setOnClickListener {
            val intent = Intent(this, QRCodeActivity::class.java)
            startActivityForResult(intent, 16)

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
            resultData?.data?.also { uri ->
                resumeUri = uri
                GlobalScope.launch {
                    startDownload(serverInformation, uri)
                }
            }
        }

        if (requestCode == 15 && resultCode == Activity.RESULT_OK) {
            var jsonHostData = resultData?.extras?.getString("result") ?: ""
            val jsonRoot = JSONObject(jsonHostData)
            hostInformation = HostInformation(jsonRoot.optString("IpAddress"),
                                              jsonRoot.getInt("Port"),
                                              jsonRoot.optBoolean("IsSsidOn"),
                                              jsonRoot.optString("SsidName"),
                                              jsonRoot.optString("SsidPassword"),
                                              jsonRoot.optString("HostIpAddress"),)
            ipAddress?.setText(hostInformation?.IpAddress)
            port?.setText(hostInformation?.Port.toString())
        }

        if (requestCode == 16 && resultCode == Activity.RESULT_OK) {
            var jsonHostData = resultData?.extras?.getString("result") ?: ""
            val jsonRoot = JSONObject(jsonHostData)
            hostInformation = HostInformation(jsonRoot.optString("IpAddress"),
                                              jsonRoot.getInt("Port"),
                                              jsonRoot.optBoolean("IsSsidOn"),
                                              jsonRoot.optString("SsidName"),
                                              jsonRoot.optString("SsidPassword"),
                                              jsonRoot.optString("HostIpAddress"),)
            ipAddress?.setText(hostInformation?.IpAddress)
            port?.setText(hostInformation?.Port.toString())
            if (peers.count() >= 1) {
                connect()
            }
        }
    }

    fun wifiDirect() {
        GlobalScope.launch(Main) {
            receiveButton?.isEnabled = false;
            serverInformation = ServerInformation(hostInformation?.HostIpAddress.toString(), hostInformation?.Port.toString().trim().toInt())
            createFile()
        }
    }

    public suspend fun startResumeDownload(serverInformation: ServerInformation, uri: Uri) {
        for (i in failedDownloadedPackage..fileInformation.SequenceCount-1) {
            getResponseFromSocket(serverInformation, AllowedPaths.SendData, i.toString())
                .let { fileInformation.FileData.add(it) }
        }

        hashManager()
        alterDocument(uri)
    }

    public suspend fun startDownload(serverInformation: ServerInformation, uri: Uri) {
        for (i in 0..fileInformation.SequenceCount-1) {
            getResponseFromSocket(serverInformation, AllowedPaths.SendData, i.toString())
                .let { fileInformation.FileData.add(it) }
        }

        hashManager()
        alterDocument(uri)
    }

    private suspend fun hashManager() {
        getHash()
        calculateHash()
        compareHash()
        replaceFileData()
    }

    private suspend fun getHash() {
        for (i in 0..fileInformation.HashSequenceCount-1) {
            getResponseFromSocket(serverInformation, AllowedPaths.HashInformation, i.toString())?.decodeToString()
                ?.let { hashInformation.hashSequenceDictionary.put(i, it.replace("\u0000".toRegex(), "")) }
        }
    }

    private fun calculateHash() {
        for (i in 0..fileInformation.SequenceCount-1) {
            val md = MessageDigest.getInstance("SHA-256")
            val digest = md.digest(fileInformation.FileData[i])
            hashComputed.put(i, digest.fold("", { str, it -> str + "%02x".format(it)}))
        }
    }

    private fun compareHash() {
        for (i in 0..fileInformation.SequenceCount-1) {
            if (hashInformation.hashSequenceDictionary[i] != hashComputed[i]) {
                downloadAgain.add(i)
            }
        }
    }

    private suspend fun replaceFileData() {
        downloadAgain.forEach {
            fileInformation.FileData[it] = getResponseFromSocket(serverInformation, AllowedPaths.SendData, it.toString())
        }
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
        if(fileInformation.Filename.length > 1)
        {
            val intent = Intent(Intent.ACTION_CREATE_DOCUMENT).apply {
                addCategory(Intent.CATEGORY_OPENABLE)
                type = "application/${fileInformation.FileExtension}"
                putExtra(Intent.EXTRA_TITLE, fileInformation.Filename)
            }
            startActivityForResult(intent, 49153)
        }
        else {
            GlobalScope.launch(Main) {
                Toast.makeText(this@MainActivity , "No Meta Data received, please try later again ", Toast.LENGTH_LONG)
                receiveButton?.isEnabled = true;
            }
        }
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
        if(response.length > 1 )
        {
            val jsonRoot = JSONObject(response)
            return FileInformation(jsonRoot.optString("Filename"),
                jsonRoot.optString("FileExtension"),
                jsonRoot.getInt("FileSize"),
                jsonRoot.getInt("BufferSize"),
                jsonRoot.getInt("SequenceCount"),
                jsonRoot.getInt("HashSequenceCount"))
        }
        return FileInformation("", "", 0, 0, 0, 0)
    }

private suspend fun getResponseFromSocket(serverInformation: ServerInformation, path : AllowedPaths, resource : String = "") : ByteArray? {
    try {
        val response = withContext(IO) {
            val exec = Executors.newCachedThreadPool()
            val selector = ActorSelectorManager(exec.asCoroutineDispatcher())
            val socket = aSocket(selector).tcp().connect(InetSocketAddress(serverInformation.ipAddress, serverInformation.port))
            val input : ByteReadChannel  = socket.openReadChannel()
            val output: ByteWriteChannel = socket.openWriteChannel(autoFlush = true)

            output.writeFully("Get /$path/$resource/".toByteArray())
            var responseArray = ByteArray(10000)
            input.readAvailable(responseArray)

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
    } catch (ex: NetworkErrorException) {
        Log.e("Network Error", "Excption", ex)
        if (resource.length > 1 && path == AllowedPaths.SendData) {
            failedDownloadedPackage = resource.toInt()
        }
        return null
    } catch (ex: ConnectException) {
        Log.e("ConnectException", "Excption", ex)
        if (resource.length > 1 && path == AllowedPaths.SendData) {
            failedDownloadedPackage = resource.toInt()
        }
        return null
    }
}

    fun discover() {
        if (ActivityCompat.checkSelfPermission(
                this,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) != PackageManager.PERMISSION_GRANTED
        ) {

            return
        }
        manager?.discoverPeers(channel, object : WifiP2pManager.ActionListener {

            override fun onSuccess() {
                //Toast.makeText(this@MainActivity, "WIFI Direct found", Toast.LENGTH_SHORT).show()
                println("discovery started")

            }

            override fun onFailure(reasonCode: Int) {
                //Toast.makeText(this@MainActivity, "Wifi Direct not found $reasonCode", Toast.LENGTH_SHORT).show()
                println("discovery not started")

            }
        })
    }

    fun connect() {
        // Picking the first device found on the network.
        var device = peers[0]
        peers.forEach {
            if(it.deviceName.contains("laptop", true)) {
                device = it
            }
        }

        val config = WifiP2pConfig().apply {
            deviceAddress = device.deviceAddress
            wps.setup = WpsInfo.PBC
        }

        if (ActivityCompat.checkSelfPermission(
                this,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) != PackageManager.PERMISSION_GRANTED
        ) {
            return
        }
        manager?.connect(channel, config, object : WifiP2pManager.ActionListener {

            override fun onSuccess() {
                // WiFiDirectBroadcastReceiver notifies us. Ignore for now.
                println("Connected.")
                wifiDirect()
            }

            override fun onFailure(reason: Int) {
                println("Connect failed. Retry. $reason")
            }
        })
    }

    val peerListListener = WifiP2pManager.PeerListListener { peerList ->
        val refreshedPeers = peerList.deviceList
        if (refreshedPeers != peers) {
            peers.clear()
            peers.addAll(refreshedPeers)

            // If an AdapterView is backed by this data, notify it
            // of the change. For instance, if you have a ListView of
            // available peers, trigger an update.

            // Perform any other updates needed based on the new list of
            // peers connected to the Wi-Fi P2P network.
        }

        if (peers.isEmpty()) {
            Log.d("TAG", "No devices found")
            return@PeerListListener
        }

    }

    /* register the broadcast receiver with the intent values to be matched */
    override fun onResume() {
        super.onResume()
        receiver?.also { receiver ->
            registerReceiver(receiver, intentFilter)
        }
    }

    /* unregister the broadcast receiver */
    override fun onPause() {
        super.onPause()
        receiver?.also { receiver ->
            unregisterReceiver(receiver)
        }
    }
}




