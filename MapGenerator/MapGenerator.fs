namespace MapGenerator
open System.Drawing
open System.Drawing.Drawing2D
open Types.Parsing

module Generate =
    ///Normalize a point into the map
    let normalize b size n =
        let calcul axe =
            let min = axe(b.min)
            if min<=axe(n)&&axe(n)<=axe(b.max) then
                let ratio = (axe(n)-min)/(axe(b.max)-min)
                int(axe(size)*ratio)
            else 
                1 ///Some points are outside the bounds
        ///Hack because the latitude is inversed
        let inline mirror axe =
            (axe>>int)(size) - calcul axe
        calcul lon, mirror lat

    let generateData relations colorMap polygonFunc lineFunc =
        let draw (x:Relation) =
            let func = if x.isMultipolygon then polygonFunc else lineFunc
            match Map.tryFind(x.relationType) colorMap with
            |Some(c)->func c x
            |None->func Color.Black x
        Seq.iter draw relations

    ///Generate a polygon from a relation
    let generateMultipolygon norm relation = 
        let toPoint p = 
            let x,y = norm p
            new Point(x,y)
        let selectNodes = function
            |Way(x, y)-> Some(x, y)
            |_-> None

        let createPolygon list =
            let toRegion (way,_) =
                let gp = new GraphicsPath()
                Seq.map(fun node->toPoint node.point) way.nodes |> Seq.toArray |> gp.AddPolygon
                new Region(gp)
            List.map(toRegion) list
        let inline apply func (x,y) =
            func x, func y

        ///The outers polygons represent the polygon we want to draw. The inners polygon are the holes in that outer polygon
        let outers, inners = 
            Seq.toList relation.members 
            |> Seq.choose(selectNodes) |> Seq.toList 
            |> List.partition(fun (_,role)->role="outer") |> apply createPolygon

        ///Union between two region
        let union (region1:Region) (region2:Region) =
            region1.Union(region2)
            region1

        ///Region1 except region2
        let xor (region1:Region) (region2:Region) =
            region1.Xor(region2)
            region1

        ///We want to "merge" all the outers polygon to create a big one, then remove all the inners one to get our final polygon
        List.fold(xor) (List.reduce(union) outers) inners

    let picture parseData =
        let size = 1000.0,1000.0
        let bitmap = new Bitmap(int(lon size), int(lat size))
        let norm = normalize parseData.bound size
        let toPoint p = 
            let x,y = norm p
            new Point(x,y)
        let drawNode color node =
            let (x, y) = (norm node.point)
            bitmap.SetPixel(x,y,color)
        let drawAll color =
            function
            |Node(item,role)-> drawNode color item
            |Way(item,role)-> Seq.iter (drawNode color) item.nodes

        let drawRoad (color:Color) relation =
            let draw x=
                let drawLine nodes =
                    let gp = new GraphicsPath()
                    let points = Seq.map(fun x->toPoint x.point) nodes |> Seq.toArray
                    using(Graphics.FromImage(bitmap))(fun g->g.DrawLines(new Pen(color),points))
                match x with
                |Node(item,role)-> drawNode color item
                |Way(item,role)-> drawLine item.nodes
            Seq.iter(draw) relation.members
        
        let drawMultipolygon color relation = 
            using(Graphics.FromImage(bitmap))(fun g->g.FillRegion(new SolidBrush(color),generateMultipolygon norm relation))

        let colorMap = [|(Forest,Color.Green);(Water,Color.Blue);(Wetland,Color.Coral); (Beach,Color.Beige)|] |> Map.ofArray

        generateData parseData.relations colorMap drawMultipolygon drawRoad
        bitmap
