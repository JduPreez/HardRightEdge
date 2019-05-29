namespace HardRightEdge.WebApi

open HardRightEdge.Infrastructure.Serialization
open Suave
open Suave.Operators
open Suave.Successful


[<AutoOpen>]
module Restful =    
  open Suave.RequestErrors
  open Suave.Filters
  open Suave.Writers
  open System.IO
  open System.Text
  
  let setCORSHeaders =
    addHeader  "Access-Control-Allow-Origin" "*" 
    >=> setHeader "Access-Control-Allow-Headers" "token" 
    >=> addHeader "Access-Control-Allow-Headers" "content-type" 
    >=> addHeader "Access-Control-Allow-Methods" "GET,OPTIONS,POST,PUT" 

  let allowCors : WebPart =
    choose [
      GET >=>
        fun context ->
          context |> (setCORSHeaders)
      OPTIONS >=>
          fun context ->
          context |> (setCORSHeaders)
      POST >=>
          fun context ->
          context |> (setCORSHeaders)
      PUT >=>
          fun context ->
          context |> (setCORSHeaders)
      DELETE >=>
          fun context ->
          context |> (setCORSHeaders)
    ]

  // 'a -> WebPart
  let inline JSON< ^T> (v: ^T) =     
    toJson v
    |> OK 
    >=> Writers.setMimeType "application/json; charset=utf-8"

  //let fromJson json =    
    //Compact.deserialize(json) |> unbox

  let inline getResourceFromReq (req : HttpRequest) : 't = 
    let getString rawForm = Encoding.UTF8.GetString(rawForm)
    let obj : 't = req.rawForm |> getString |> fromJson
    obj

  let inline getResourceFromReqFiles (req: HttpRequest) : byte [] =
    req.rawForm
    (*match req.files with
    | head :: _ -> File.ReadAllBytes(head.tempFilePath)
    | [] -> Array.empty<byte>*)


  type RestRes<'a> =
    | GetAll of (unit -> 'a list)
    | Create of ('a list -> 'a list)
    | Upload of (byte [] -> 'a list)
    | GetById of (int64 -> 'a option)
    | IsExists of (int64 -> bool)
    | Update of ('a list -> 'a list)
    | UpdateById of (int64 -> 'a -> 'a)
    | Delete of (int64 -> unit)

  let inline rest resourceName (resources: RestRes<'a> list) =
    let resourcePath = "/" + resourceName
    let resourceIdPath = new PrintfFormat<(int64 -> string),unit,string,string,int64>(resourcePath + "/%d")
      
    //let badRequest = BAD_REQUEST "Resource not found"

    let handleResource requestError = function
        | Some r -> r |> JSON
        | _ -> requestError
     
    let toWebParts (webParts: ((HttpContext -> Async<HttpContext option>) list) * ((HttpContext -> Async<HttpContext option>) list)) method =
      ((match method with
        | GetAll getAll ->           
          (GET >=> warbler (fun _ -> getAll () |> JSON)) :: fst webParts
        | Create create ->           
          (POST >=> request (getResourceFromReq >> create >> JSON)) :: fst webParts
        | Update update -> 
          (PUT >=> request (getResourceFromReq >> update >> JSON)) :: fst webParts
        | Upload upload ->
          (POST >=> request (getResourceFromReqFiles >> upload >> JSON)) :: fst webParts
        | _ -> fst webParts), 
        match method with
        | GetById(getById) ->        
          (GET >=> pathScan resourceIdPath (getById >> handleResource (NOT_FOUND "Resource not found"))) :: snd webParts
        | UpdateById(updateById) -> 
          (PUT >=> pathScan resourceIdPath (fun id -> request (getResourceFromReq >> (updateById id) >> JSON))) :: snd webParts
        | Delete(deleteById) ->
          let deleteResourceById id =
                      deleteById id
                      NO_CONTENT
          (DELETE >=> pathScan resourceIdPath deleteResourceById) :: snd webParts
        | IsExists(isExists) ->
          (HEAD >=> pathScan resourceIdPath (fun id -> if isExists id then OK "" else NOT_FOUND "")) :: snd webParts
        | _ -> snd webParts)
    
    let (batchWebParts, singleWebParts) = List.fold toWebParts ([], [(OPTIONS >=> warbler (fun _ -> OK ""))]) resources

    allowCors >=> choose ((path resourcePath >=> choose batchWebParts) :: singleWebParts)

    (*let isGet r = match r with | GetAll(_) -> true | _ -> false
    let get = match List.tryFind isGet resources with
              | Some (GetAll(getAll)) ->  [GET >=> warbler (fun _ -> getAll () |> JSON)]
              | _ -> []
    
    let isPost r = match r with | Create(_) -> true | _ -> false
    let post =  match List.tryFind isPost resources with
                | Some create -> 
                  let x = unbox create
                  [POST >=> request (getResourceFromReq >> x >> JSON)]
                | _ -> []

    let put = match resource.Update with
              | Some update -> [PUT >=> request (getResourceFromReq >> update >> JSON)]
              | _ -> []

    let delete1 = match resource.Delete with
                  | Some delete -> 
                    let deleteResourceById id =
                      delete id
                      NO_CONTENT

                    [DELETE >=> pathScan resourceIdPath deleteResourceById]
                  | _ -> []

    let get1 =  match resource.GetById with
                | Some getById ->
                  let getResourceById = 
                    getById >> handleResource (NOT_FOUND "Resource not found")

                  [GET >=> pathScan resourceIdPath getResourceById]
                | _ -> []

    let put1 =  match resource.UpdateById with
                | Some updateById ->
                  let updateResourceById id =
                    request (getResourceFromReq >> (updateById id) >> JSON)

                  [PUT >=> pathScan resourceIdPath updateResourceById]
                | _ -> []

    let head = match resource.IsExists with
                | Some isExists -> 
                  let isResourceExists id =
                    if isExists id then OK "" else NOT_FOUND ""

                  [HEAD >=> pathScan resourceIdPath isResourceExists]
                | _ -> []*)

    //path resourcePath >=> GET >=> get.Head

        //[
        //[
          //delete1 @ get1 @ put1 @ head
              //GET >=> getAll
              //POST >=> request (getResourceFromReq >> resource.Create >> JSON)
              //PUT >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
          //]
          //DELETE >=> pathScan resourceIdPath deleteResourceById
          //GET >=> pathScan resourceIdPath getResourceById
          //PUT >=> pathScan resourceIdPath updateResourceById
          //HEAD >=> pathScan resourceIdPath isResourceExists
      //]