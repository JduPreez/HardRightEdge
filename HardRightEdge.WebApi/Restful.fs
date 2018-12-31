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
  let JSON v =     
    (*let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.Converters.Add(new IdiomaticDuConverter())
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    jsonSerializerSettings.NullValueHandling <- NullValueHandling.Ignore

    JsonConvert.SerializeObject(v, jsonSerializerSettings)*)
    //Compact.serialize(v)
    toJson v
    |> OK 
    >=> Writers.setMimeType "application/json; charset=utf-8"

  //let fromJson json =    
    //Compact.deserialize(json) |> unbox

  let getResourceFromReq (req : HttpRequest) = 
    let getString rawForm = 
      let x = System.Text.Encoding.UTF8.GetString(rawForm)
      x

    req.rawForm |> getString |> fromJson<HardRightEdge.Domain.Security>

  type RestResource<'a> = {
    GetAll      : (unit -> 'a list) option
    GetById     : (int64 -> 'a option) option
    IsExists    : (int64 -> bool) option
    Create      : ('a -> 'a) option
    Update      : ('a -> 'a) option
    UpdateById  : (int64 -> 'a -> 'a) option
    Delete      : (int64 -> unit) option
  }

  let rest resourceName (resource: RestResource<'a>) =
     
    let resourcePath = "/" + resourceName
    let resourceIdPath = new PrintfFormat<(int64 -> string),unit,string,string,int64>(resourcePath + "/%d")
      
    let badRequest = BAD_REQUEST "Resource not found"

    let handleResource requestError = function
        | Some r -> r |> JSON
        | _ -> requestError

    let get = match resource.GetAll with
              | Some getAll ->  [GET >=> warbler (fun _ -> getAll () |> JSON)]
              | _ -> []

    let post =  match resource.Create with
                | Some create -> [POST >=> request (getResourceFromReq >> create >> JSON)]
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
                | _ -> []

    //path resourcePath >=> GET >=> get.Head

    let z = get @ post @ put
    allowCors >=> choose ((path resourcePath >=> choose z) :: (delete1 @ get1 @ put1 @ head))

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