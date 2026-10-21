**Unittest handler logik**

Unittesten for messagehandler logikken er lavet med Moq og Xunit bibliotekerne. 
Efter lidt undersøgelse, virkede Moq interessant og simpel i sin opsætning, og virkede kodebesparende, så det skulle prøves.

Først så laves objekterne til dbContext og en flaks messageClient, som sættes op til at skulle rapportere Task.Complete når en besked er published. 

Testen den foregår ved at databasen seedes via EF. Så oprettes en besked med det samme channel.Id som lige blev genereret, så handleren kan have noget at tjekke op imod for success, eller lav et nyt guid i message til channel.Id for at se det fejle. 

Samtidig så kører messagehandleren en optælling af nye beskeder, så frontenden kan lave farveskift for den channel der er nye beskeder i.

Efter beskeden er published, så tjekker unittesten om det der er published er det samme som den besked der er sendt, samt at nummeret af beskeder også er steget til 1, og at det hele sker 1 gang. 

Beskeden er formet efter MessagedPostedEvent som er en delt model imellem micro services, og har alle de nødvendige getter og setter for at beskeden har en kontrakt. 

Tests køres med denne kommando:

```sh dotnet test tests/UnitTests ```

