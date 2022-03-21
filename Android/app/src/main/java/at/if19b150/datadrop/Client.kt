package at.if19b150.datadrop

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.OutputStream
import java.net.Socket
import java.util.*
import kotlin.concurrent.thread

class Client(var address : String, var port : Int, var filepath: String = "") {
    var connection : Socket? = null
    var reader : Scanner? = null
    var writer : OutputStream? = null
    var connected : Boolean = false
    var stringDings : String = ""

    suspend fun start() {
        val response = withContext(Dispatchers.IO){

            connection = Socket("10.0.0.40", 49153)
            reader = Scanner(connection?.getInputStream())
            writer = connection?.getOutputStream()
            connected = true
            //thread { readFromServer()  }
            /*while (connected){

            }*/

            writeToServer(AllowedPaths.DataInformation)
            //thread { readFromServer()  }
            readFromServer()
            stringDings += 3

        }

        println(response)
        stringDings += 5
    }

    private fun writeToServer(allowedPath : AllowedPaths, resource : String = "") {
        println("Write to $address:$port ")
        println("Message :${allowedPath.name}/$resource")
        writer?.write("${allowedPath.name}/$resource".toByteArray(Charsets.UTF_8))
    }

    private fun readFromServer() {
        stringDings += reader?.nextLine()
        println("Received: $stringDings")
        var aaa = 1
        /*while (connected)
        {
            stringDings += reader?.nextLine()
            var aaa = 1
        }*/
    }

}