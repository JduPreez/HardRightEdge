module Domain.Currencies exposing (Currency)

type Currency
  = USD Int String
  | EUR Int String
  | GBP Int String
  | SGD Int String
  | DKK Int String

usd = USD 1 "USD"
eur = EUR 2 "EUR"
gbp = GBP 3 "GBP"
sgd = SGD 4 "SGD"
dkk = DKK 5 "DKK"