**Database** 

Vi har valgt at bruge den nyeste version af mssql database, og vi bruger docker til at køre den igennem, da det er noget vi alle kendte, og det er godt understøttet med dokumentation.

Det er også en database som er godt understøttet af microsoft, og vi kan nemt hente images direkte fra dem. 
Vi kører dog med latest database i vores docker compose, så vi får alle de nye features med i vores database, og derved kan holde fingeren på pulsen i ny teknologi på databasefronten. 

Dette gør at vi nemt kan skalere på x-aksen, og gør at vi hurtigt kan opsætte samme database på andre pc’ere og servere.   
