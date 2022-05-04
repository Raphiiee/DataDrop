package at.if19b150.datadrop

import org.json.JSONArray

class FileInformation(var Filename : String, var FileExtension : String, var FileSize : Int, var BufferSize : Int, var SequenceCount : Int, var HashSequenceCount : Int?) {
    var FileData = mutableListOf<ByteArray?>()

    var HashSequenceList = mutableListOf<String>()

}