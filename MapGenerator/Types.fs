namespace MapGenerator
module Types =
    module Parsing =
        ///Longitude, Latitude
        type point = float*float
        let lon = fst
        let lat = snd

        ///Help define the meaning of the element attached
        type Tag = {
            k:string;
            v:string;
        }
        ///Defining points in space
        type Node = {
            point:point;
            tags:seq<Tag>
        }
        ///Defining linear features and area boundaries
        type Way = {
            nodes:seq<Node>
            tags:seq<Tag>
        }
        ///Bounds of the current map
        type Bound = {
            min:point;
            max:point;
        }

        ///A member in a relation
        type RelationMember =
            |Node of item: Node * role:string
            |Way of item: Way * role:string

        ///Enum to define the type of relation (is this element a forest or a beach?)
        type RelationType =
            |Forest
            |Water
            |Wetland
            |Beach
            |Unsupported
            
        ///Which are sometimes used to explain how other elements work together
        type Relation = {
            members:seq<RelationMember>
            tags:seq<Tag>
            relationType:RelationType
            isMultipolygon:bool
        }
        ///Results from the file parsing
        type ParsingData ={
            relations: seq<Relation>
            nodeMap: Map<uint64,Node>
            wayMap: Map<uint64,Way>
            bound: Bound
        }

