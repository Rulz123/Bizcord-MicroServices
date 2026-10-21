## Unittest handler logic

The unit test for the MessageHandler logic is implemented using the Moq and xUnit libraries.

After doing some research, Moq seemed interesting and simple to set up. It also appeared to reduce the amount of code required, so I decided to give it a try.

First, the objects for the DbContext and a fake messageClient are created. The messageClient is configured to return Task.CompletedTask when a message is published.

The test works by seeding the database using EF. A message is then created with the same channel.Id that was just generated, allowing the handler to verify a successful result. Alternatively, a new GUID can be assigned to the message's channel.Id to test the failure scenario.

At the same time, the MessageHandler keeps track of the number of new messages, allowing the frontend to change the color of the channel that contains new messages.

After the message has been published, the unit test verifies that the published message matches the message that was sent. It also checks that the message count has increased to 1 and that the entire operation has been executed exactly once.

The message is structured according to MessagePostedEvent, which is a shared model between microservices and contains all the necessary getters and setters to ensure that the message adheres to the defined contract.

The tests are run using the following command:

```sh dotnet test tests/UnitTests ```

