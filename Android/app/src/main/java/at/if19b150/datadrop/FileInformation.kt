package at.if19b150.datadrop

import android.os.Parcel
import android.os.Parcelable

class FileInformation(var Filename : String, var FileExtension : String, var FileSize : Int, var BufferSize : Int) {
    var SequenzeCount : Int
    get() {
        if (FileSize / BufferSize > 0){
            return FileSize / BufferSize
        }
        return 0
    }
    set(value) {
        SequenzeCount = value
    }

    var FileData = mutableListOf<ByteArray?>()

}