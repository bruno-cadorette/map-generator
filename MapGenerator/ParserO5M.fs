namespace MapGenerator
//open System
//open System.IO
//open System.Text
//open Types.Parsing
//
//module Binairy =
//    type o5mReader(data:byte[]) =
//        member this.count = 0;
//        member this.data = data
//        member this.readInt() =
//            let rec assemble i =
//                let element = int64(data.[count+i])
//                if (element < 0x80L) then
//                    count <- count + i
//                    int64(element<<<i*7)
//                else
//                    (element &&& 0x7FL) ||| assemble(i+1)
//            assemble 0
//
//        member this.readString() =
//            let stringStream =
//                Seq.initInfinite(fun i-> char(data.[count+i]))
//                |> Seq.takeWhile(fun c-> c<>(char)0)
//                |> Seq.fold(fun (sb:StringBuilder) c-> sb.Append(c)) (new StringBuilder())
//            let str = stringStream.ToString()
//            count <- count+str.Length
//            str
//
//    let createTag(reader:o5mReader) =
//            {k=reader.readString();v=reader.readString()}
//
//    let o5mParse url =
//        use reader = new BinaryReader(File.OpenRead url)
//        let readNode arr = 
//            let data = new o5mReader(arr)
//            let id = data.readInt() |> Convert.ToUInt64
//            let version =
//                let versionNumber = data.readInt()
//                if versionNumber <> 0L then
//                    data.readInt() |> ignore
//                    createTag(data) |> ignore
//            let longitude = data.readInt() |> Convert.ToDouble
//            let lattitude = data.readInt() |> Convert.ToDouble
//            let tags = Seq.initInfinite(fun i->createTag data) |> Seq.takeWhile(fun x->data.count<data.data.Length)
//
//            id, {point=longitude,lattitude;tags=tags}
//        let readWay x = printfn "readWay"
//        let readRelation x = printfn "readRelation"
//        let readBoundingBox x = printfn "readBoundingBox"
//        let readFileStamp x = printfn "readFileStamp"
//        let readHeader x = printfn "readHeader"
//        let sync() = printfn "sync"
//        let jump() = printfn "jump"
//        let reset() = printfn "reset"
//        let readNodes(reader:BinaryReader) =
//            Seq.initInfinite(fun i->i,"all") |> Seq.takeWhile(fun i->reader.ReadByte()=0x10uy) |> Map.ofSeq
//
//        let rec readDataSet() =
//            let eof() =
//                reader.BaseStream.Position = reader.BaseStream.Length
//            let bytes() = int(reader.ReadByte()) |> reader.ReadBytes
//            match reader.ReadByte() with
//            |0x10uy-> readNode(bytes())
//            |0x11uy-> readWay(bytes())
//            |0x12uy-> readRelation(bytes())
//            |0xdbuy-> readBoundingBox(bytes())
//            |0xdcuy-> readFileStamp(bytes())
//            |0xe0uy-> readHeader(bytes())
//            |0xeeuy-> sync()
//            |0xefuy-> jump()
//            |0xffuy-> reset()
//            |n-> failwith ("Unexpected dataset " + Convert.ToString(n))
//            if not(eof()) then readDataSet()
//        readDataSet()
//
//
