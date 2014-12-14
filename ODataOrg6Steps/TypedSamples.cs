using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.OData.Client;
using Xunit;

namespace ODataOrg6Steps
{
    public class TypedSamples
    {
        public TypedSamples()
        {
        }

        [Fact]
        public async Task Step1()
        {
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var people = await client
                .For<People>()
                .FindEntriesAsync();
            Assert.Equal(8, people.Count());
        }

        [Fact]
        public async Task Step1_WithAnnotations()
        {
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var annotations = new ODataFeedAnnotations();
            var count = 0;
            var people = await client
                .For<People>()
                .FindEntriesAsync(annotations);
            count += people.Count();
            while (annotations.NextPageLink != null)
            {
                people = await client
                    .For<People>()
                    .FindEntriesAsync(annotations.NextPageLink, annotations);
                count += people.Count();
            }
            Assert.Equal(20, count);
        }

        [Fact]
        public async Task Step2()
        {
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var person = await client
                .For<People>()
                .Key("russellwhyte")
                .FindEntryAsync();
            Assert.Equal("russellwhyte", person.UserName);
        }

        [Fact]
        public async Task Step3()
        {
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var people = await client
                .For<People>()
                .Filter(x => x.Trips.Any(y=> y.Budget > 3000))
                .Top(2)
                .Select(x => new {x.FirstName, x.LastName})
                .FindEntriesAsync();
            Assert.Equal(2, people.Count());
        }

        [Fact]
        public async Task Step4()
        {
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var person = await client
                .For<People>()
                .Set(new People()
                {
                    UserName = "lewisblack",
                    FirstName = "Lewis",
                    LastName = "Black",
                    Emails = new [] { "lewisblack@example.com" },
                    AddressInfo = new []
                    {
                        new Location()
                        {
                            Address = "187 Suffolk Ln.",
                            City = new City
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
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var trip = await client
                .For<People>()
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .Key(0)
                .FindEntryAsync();
            await client
                .For<People>()
                .Key("scottketchum")
                .LinkEntryAsync(trip);
            var person = await client
                .For<People>()
                .Key("scottketchum")
                .Expand(x => x.Trips)
                .FindEntryAsync();
            Assert.True(person.Trips.Any(x => x.Name == trip.Name));
        }

        [Fact]
        public async Task Step6()
        {
            var client = new ODataClient("http://services.odata.org/v4/TripPinServiceRW/");
            var people = await client
                .For<People>()
                .Key("scottketchum")
                .NavigateTo<Trip>()
                .Key(0)
                .Function("GetInvolvedPeople")
                .ExecuteAsEnumerableAsync();
            Assert.Equal(2, people.Count());
        }
    }
}
