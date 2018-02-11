using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestWebApiAzure.Controllers
{
    public class Family
    {
        [JsonProperty(PropertyName = "id")]
        public string Id;
        public Parent[] parents;
        public Child[] children;
        public bool isRegistered;
    }

    public struct Parent
    {
        public string familyName;
        public string givenName;
    }

    public class Child
    {
        public string familyName;
        public string givenName;
        public string gender;
        public int grade;
        public List<Pet> pets;
    }

    public class Pet
    {
        public string givenName;
    }

    public class Address
    {
        public string state;
        public string county;
        public string city;
    }
}