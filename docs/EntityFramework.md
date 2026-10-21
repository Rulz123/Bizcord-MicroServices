## Entity framework

Vi har valgt at bruge Entity Framework, da det giver en automation via code first princippet, og vi derfor ikke selv skal oprette databasen. Det er også et værktøj som vi er rimeligt kendte med, og det giver os en stor besparelse på databehandlings kode, da det virker perfekt med linq.
Der er også masser af dokumentation for Entity Framework, hvilket gør det nemt at implementere for både nye og mere erfarne udviklere.

Dette valg er taget, da det er en ret simpel database, og vi nemt kan holde styr på migrationers versionering, og nemt kan skalere med det.
Nu hvor vi bruger EF ( Entity Framework), så behøver vi slet ikke at skrive selve querys, da dbContext og EF gør det for os. Det virker også perfekt med asynkron funktionalitet, og gør det mega hurtigt at udvide og arbejde sammen med og i andre services. 
Det virker også perfekt med linq fremgangsmåden, og samler databaseudtrækkene som collections. 

Når der er en frisk instans af microservicen, så skal der laves en migration og derefter en update database, så databsen bliver lavet via entity framework. 

Dette gøres med følgende kommandoer:

```sh 
dotnet ef migrations add DatabaseV_0
dotnet ef database update
```