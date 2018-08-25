namespace HardRightEdge.WebApi

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Http
open Suave.Successful


[<AutoOpen>]
module Restful =    
    open Suave.RequestErrors
    open Suave.Filters    

    // 'a -> WebPart
    let JSON v =     
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    
        JsonConvert.SerializeObject(v, jsonSerializerSettings)
        |> OK 
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json =
        JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a    

    let getResourceFromReq<'a> (req : HttpRequest) = 
        let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    type RestResource<'a> = {
        GetAll      : (unit -> 'a list) option
        GetById     : (int -> 'a option) option
        IsExists    : (int -> bool) option
        Create      : ('a -> 'a) option
        Update      : ('a -> 'a option) option
        UpdateById  : (int -> 'a -> 'a option) option
        Delete      : (int -> unit) option
    }

    let rest<'a> resourceName (resource: RestResource<'a>) =
       
      let resourcePath = "/" + resourceName
      let resourceIdPath = new PrintfFormat<(int -> string),unit,string,string,int>(resourcePath + "/%d")
        
      let badRequest = BAD_REQUEST "Resource not found"

      let handleResource requestError = function
          | Some r -> r |> JSON
          | _ -> requestError

      let get = match resource.GetAll with
                | Some getAll -> [GET >=> warbler (fun _ -> getAll () |> JSON)]
                | _ -> []

      let post =  match resource.Create with
                  | Some create -> [POST >=> request (getResourceFromReq >> create >> JSON)]
                  | _ -> []

      let put = match resource.Update with
                | Some update -> [PUT >=> request (getResourceFromReq >> update >> handleResource badRequest)]
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
                      request (getResourceFromReq >> (updateById id) >> handleResource badRequest)

                    [PUT >=> pathScan resourceIdPath updateResourceById]
                  | _ -> []

      let head = match resource.IsExists with
                  | Some isExists -> 
                    let isResourceExists id =
                      if isExists id then OK "" else NOT_FOUND ""

                    [HEAD >=> pathScan resourceIdPath isResourceExists]
                  | _ -> []

      path resourcePath >=> GET >=> get.Head

      //let z = get @ post @ put
      //choose ((path resourcePath >=> choose z) :: (delete1 @ get1 @ put1 @ head))

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