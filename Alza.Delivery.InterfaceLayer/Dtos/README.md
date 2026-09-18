# DTOs generation

- Generating DTOs of API via nswag cli
- Generation is without clients
- run from Dtos dir
```bash
nswag openapi2csclient /input:../../Contracts/alza-delivery-api.yaml /output:Dtos.Generated.cs /namespace:Alza.Contracts /generateClientClasses:false /generateClientInterfaces:false /generateDtoTypes:true /generateExceptionClasses:false /typeAccessModifier:public /classStyle:Poco
```
