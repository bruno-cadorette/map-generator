namespace MapGenerator
open System
open System.Xml
open System.Xml.Linq

open Types.Parsing
///Do not use for huge files, unless you have a shitload of ram. It's fine with prince edward island (174 Mb)
module Parser =
    module private XML =
        ///Get one attribute from the element. We still have to cast the result
        let inline attr (elem:XElement) att =
            elem.Attribute(XName.Get att)
        ///Get all the childs of the type given
        let inline elements item (data:XElement) =
           data.Elements(XName.Get item)
    let private createTag x =
        {k=(XML.attr x "k").Value; v = (XML.attr x "v").Value}
            
    let private createBound x =
        let min = (float)(XML.attr x "minlon"), (float)(XML.attr x "minlat")
        let max = (float)(XML.attr x "maxlon"), (float)(XML.attr x "maxlat")
        {min=min;max=max}
    
    let inline private getAllTags item =
         XML.elements "tag" item |> Seq.map createTag

    let getAllNodes item =
        let createNode x =
            let id = (uint64)(XML.attr x "id")
            let lon = (float)(XML.attr x "lon")
            let lat = (float)(XML.attr x "lat")
            
            id, {point = lon,lat; tags= getAllTags item}
        XML.elements "node" item |> Seq.map createNode |> Map.ofSeq
    
    
    ///Get all the relations in the xml data.
    let getAllRelations data ways nodes = 
        ///Define the kind of the relation, is this relation reprensent a forest or a beach?
        let getType tags =
            let supportedTags = [|"natural"|]
            match Seq.tryFind(fun tag->Array.exists(fun x->tag.k=x) supportedTags) tags with
            |Some(x) when x.v = "wood" -> Forest
            |Some(x) when x.v = "water" -> Water
            |Some(x) when x.v = "wetland" -> Wetland
            |Some(x) when x.v = "beach" -> Beach
            |_->Unsupported
        let isMultipolygon =
            Seq.exists(fun x->x.v = "multipolygon")
        ///A member is a node or a way, we need to use a discriminated union here
        let getMember x =
            ///Some keys are not represented in the data structures, most of the time it's because the area represented in the file is too small
            let getItem key = 
                let role = (XML.attr x "role").Value
                match (XML.attr x "type").Value with
                |"way"-> match Map.tryFind key ways with
                            |Some(x)->Some(Way(item = x, role = role))
                            |None->None
                |"node" -> match Map.tryFind key nodes with
                            |Some(x)->Some(Node(item = x, role = role))
                            |None->None
                |"relation"->None
                |n->failwith ("Unexpection relation member" + n)
            getItem(uint64(XML.attr x "ref"))
        XML.elements "relation" data |> Seq.map(fun item->
            let tags = getAllTags item
            {members= XML.elements "member" item |> Seq.choose(getMember); 
            tags= tags; relationType = getType(tags); isMultipolygon = isMultipolygon tags}
        )

    ///Parse an OSM file into data 
    let xmlParse(url:string) =
        let data = XDocument.Load(url).Root
        let bound = data.Element(XName.Get "bounds") |> createBound
        let nodes = getAllNodes data
        
        let createWay x =
            let id = (uint64)(XML.attr x "id")
            id, {nodes = (XML.elements "nd" x |> Seq.map(fun x->Map.find((uint64)(XML.attr x "ref")) nodes));
             tags= getAllTags x}

        let ways = XML.elements "way" data |> Seq.map(createWay) |> Map.ofSeq
        let relations = getAllRelations data ways nodes
        {relations= relations; wayMap=ways; nodeMap=nodes; bound =bound}

    