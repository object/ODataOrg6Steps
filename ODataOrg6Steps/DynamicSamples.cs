using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.OData.Client;
using Xunit;

namespace ODataOrg6Steps
{
    public class DynamicSamples
    {
        public DynamicSamples()
        {
        }

        [Fact]
        public async Task Step1()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var people = await client
                .For(x.People)
                .FindEntriesAsync() as IEnumerable<dynamic>;
            Assert.Equal(8, people.Count());
        }

        [Fact]
        public async Task Step1_WithAnnotations()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var annotations = new ODataFeedAnnotations();
            var count = 0;
            var people = await client
                .For(x.People)
                .FindEntriesAsync(annotations) as IEnumerable<dynamic>;
            count += people.Count();
            while (annotations.NextPageLink != null)
            {
                people = await client
                    .For(x.People)
                    .FindEntriesAsync(annotations.NextPageLink, annotations) as IEnumerable<dynamic>;
                count += people.Count();
            }
            Assert.Equal(20, count);
        }

        [Fact]
        public async Task Step2()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var person = await client
                .For(x.People)
                .Key("russellwhyte")
                .FindEntryAsync();
            Assert.Equal("russellwhyte", person.UserName);
        }

        [Fact]
        public async Task Step3()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var people = await client
                .For(x.People)
                .Filter(x.Trips.Any(x.Budget > 3000))
                .Top(2)
                .Select(x.FirstName, x.LastName)
                .FindEntriesAsync() as IEnumerable<dynamic>;
            Assert.Equal(2, people.Count());
        }

        [Fact]
        public async Task Step4()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var person = await client
                .For(x.People)
                .Set(new 
                {
                    UserName = "lewisblack",
                    FirstName = "Lewis",
                    LastName = "Black",
                    Emails = new[] { "lewisblack@example.com" },
                    AddressInfo = new[]
                    {
                        new
                        {
                            Address = "187 Suffolk Ln.",
                            City = new
                            {
                                CountryRegion = "United States", 
                                Name = "Boise", 
                                Region = "ID"
                            }
                        }
                    },
                    Gender = PersonGender.Male,
                    Concurrency = 635519729375200400
                })
                .InsertEntryAsync();
            Assert.NotNull(person);
        }

        [Fact]
        public async Task Step5()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var trip = await client
                .For(x.People)
                .Key("russellwhyte")
                .NavigateTo(x.Trips)
                .Key(0)
                .FindEntryAsync();
            await client
                .For(x.People)
                .Key("scottketchum")
                .LinkEntryAsync(x.Trips, trip);
            var person = await client
                .For(x.People)
                .Key("scottketchum")
                .Expand(x.Trips)
                .FindEntryAsync();
            Assert.Equal(2, (person.Trips as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task Step6()
        {
            var x = ODataDynamic.Expression;

            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var people = await client
                .For(x.People)
                .Key("scottketchum")
                .NavigateTo(x.Trips)
                .Key(0)
                .Function("GetInvolvedPeople")
                .ExecuteAsEnumerableAsync() as IEnumerable<dynamic>;
            Assert.Equal(2, people.Count());
        }
    }
}
