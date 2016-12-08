namespace HardRightEdge.Services.Integration

open System
open System.Globalization

open RestSharp

open HardRightEdge.Services.Domain

module Yahoo = 

      module Fields =
        let shortName = "shortName"
        let startMonth = "startMonth"
        let startDay = "startDay"
        let startYear = "startYear"
        let endMonth = "endMonth"
        let endDay = "endDay"
        let endYear = "endYear"

      type QueryDate = {  month: string;
                          day: int }

      let private baseUrl = "http://real-chart.finance.yahoo.com"
      let private resource = sprintf "table.csv?s={%s}&a={%s}&b={%s}&c={%s}&d={%s}&e={%s}&f={%s}&g=d&ignore=.csv" 
                                      Fields.shortName 
                                      Fields.startMonth 
                                      Fields.startDay 
                                      Fields.startYear 
                                      Fields.endMonth 
                                      Fields.endDay 
                                      Fields.endYear
      

      let private queryDate (date: DateTime) =
        {   month = (date.Month - 1).ToString().PadLeft(2, '0');
            day = date.Day }

      let private fromDte date = queryDate date

      let private toDte = queryDate DateTime.Now

      let getFinancialSecurity symbol (from: DateTime) (to': unit -> DateTime) =
        let client = RestClient(baseUrl)
          
        let toVal = to'()

        let request = RestRequest(resource, Method.GET)
                        .AddUrlSegment(Fields.shortName, symbol)
                        .AddUrlSegment(Fields.startMonth, from.Month.ToString())
                        .AddUrlSegment(Fields.startDay, from.Day.ToString())
                        .AddUrlSegment(Fields.startYear, from.Year.ToString())
                        .AddUrlSegment(Fields.endMonth, toVal.Month.ToString())
                        .AddUrlSegment(Fields.endDay, toVal.Day.ToString())
                        .AddUrlSegment(Fields.endYear, toVal.Year.ToString())

          // http://real-chart.finance.yahoo.com/table.csv?s=GSK.L&a=06&b=1&c=2014&d=04&e=11&f=2015&g=d&ignore=.csv
          // http://ichart.finance.yahoo.com/table.csv?s=DNR&a=11&b=31&c=2013&d=01&e=13&f=2014&g=d&ignore=.csv
          // http://ichart.finance.yahoo.com/table.csv?s=CVX&a=00&b=2&c=2014&d=03&e=11&f=2014&g=d&ignore=.csv
          // "http://ichart.finance.yahoo.com/table.csv?s=%s&a=%s&b=%i&c=%i&d=01&e=13&f=2014&g=d&ignore=.csv"
        let response = client.Execute(request)

        let priceHistory = response.Content.Replace("r", "").Split('\n')

        { id = None;
          dataProviders = [| {  stockId = None;
                                symbol = symbol.ToUpper();
                                dataProvider = DataProvider.Yahoo } |];
          name = symbol.ToUpper();
          prices = [ for price in priceHistory.[1..] do
                                    let cols = price.Split(',')
        
                                    if Array.length cols = 7 then                                        
                                        yield { id = None;
                                                stockId = None;
                                                date = Convert.ToDateTime(cols.[0], CultureInfo.InvariantCulture);
                                                openp = Convert.ToDecimal(cols.[1], CultureInfo.InvariantCulture);
                                                high = Convert.ToDecimal(cols.[2], CultureInfo.InvariantCulture);
                                                low = Convert.ToDecimal(cols.[3], CultureInfo.InvariantCulture);
                                                close = Convert.ToDecimal(cols.[4], CultureInfo.InvariantCulture); 
                                                volume = Convert.ToInt64(cols.[5], CultureInfo.InvariantCulture);
                                                adjClose = Convert.ToDecimal(cols.[6], CultureInfo.InvariantCulture) } ] }
