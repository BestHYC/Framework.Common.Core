using Framework.Common.Core;

namespace ConsoleApp.Test.ExtensionTest
{
    public static class ExtensionTest
    {
        public static void RunJson()
        {
            Console.WriteLine("JsonExtensionTest.Run()");
            var obj = new JsonExtensionTestModel()
            {
                Name = "John",
                Age = 30,
                BirthDate = new DateTime(1990, 1, 1, 12, 0, 0)
            };
            string json = obj.ToJson(true);
            Console.WriteLine($"Serialized JSON: {json}");
            var deserializedObj = json.ToObject<JsonExtensionTestModel>();
            Console.WriteLine($"Deserialized Object: Name={deserializedObj.Name}, Age={deserializedObj.Age}, BirthDate={deserializedObj.BirthDate}");
        }
        public static void RunString()
        {
            Console.WriteLine("StringExtensionTest.Run()");
            string testString = "hello";
            string upperFirst = testString.ToUpperFirst();
            Console.WriteLine($"Original: {testString}, Upper First: {upperFirst}");
            var numbers = new List<int> { 1, 2, 3 };
            string sqlInNumbers = numbers.ToSqlIn();
            Console.WriteLine($"SQL IN for numbers: {sqlInNumbers}");
            var strings = new List<string> { "apple", "banana's", "cherry" };
            string sqlInStrings = strings.ToSqlIn();
            Console.WriteLine($"SQL IN for strings: {sqlInStrings}");
        }

        public static void RunMapper()
        {
            Console.WriteLine("Mapper.Run()");
            var model = new AModel
            {
                Id = 1,
                Namexx = "Alice",
                Nick = "Ali",
                Age = 20
            };
            var entity = model.MapTo<AEntity>();
            Console.WriteLine($"AEntity: Id={entity.Id}, Nameyyy={entity.Nameyyy}, Age={entity.Age}");

            var b = model.MapTo<BEntity>();
            Console.WriteLine($"BEntity: NameB={b.NameB}");

            var fallback = model.MapTo<FallbackEntity>();
            Console.WriteLine($"Fallback Type=null: NameDefault={fallback.NameDefault}");

            var self = model.MapTo<SelfNameEntity>();
            Console.WriteLine($"Fallback 属性名本身: Nick={self.Nick}");

            var models = new List<AModel>
            {
                model,
                new AModel { Id = 2, Namexx = "Bob", Age = 30 }
            };
            var created = models.MapTo<AEntity>();
            Console.WriteLine($"Batch create: count={created.Count}, [0]={created[0].Nameyyy}, [1]={created[1].Nameyyy}");

            var patch = new List<AEntity>
            {
                new AEntity { Nameyyy = "old1", Age = 1 },
                new AEntity { Nameyyy = "old2", Age = 2 }
            };
            models.MapTo<AEntity>(patch);
            Console.WriteLine($"Batch update: [0]={patch[0].Nameyyy}/{patch[0].Age}, [1]={patch[1].Nameyyy}/{patch[1].Age}");

            var byKey = new List<AEntity>
            {
                new AEntity { Id = 2, Nameyyy = "old2", Age = 2 },
                new AEntity { Id = 1, Nameyyy = "old1", Age = 1 }
            };
            models.MapTo(byKey, x => x.Id);
            Console.WriteLine($"Batch update by key: [0] Id={byKey[0].Id} {byKey[0].Nameyyy}/{byKey[0].Age}, [1] Id={byKey[1].Id} {byKey[1].Nameyyy}/{byKey[1].Age}");

            var convert = new ConvertModel
            {
                Id = 7,
                Amount = 12,
                Level = "3",
                Status = "Active",
                Age = null,
                Flag = 1,
                Time = 1577836800000L,
                Ignored = new DateTime(2020, 1, 1)
            }.MapTo<ConvertEntity>();
            Console.WriteLine($"Convert: Id={convert.Id}, Amount={convert.Amount}, Level={convert.Level}, Status={convert.Status}, Age={convert.Age}, Flag={convert.Flag}, Time={convert.Time:yyyy-MM-dd HH:mm:ss}, Ignored={convert.Ignored}");

            var round = new ConvertEntity
            {
                Flag = false,
                Time = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }.MapTo<ConvertBackModel>();
            Console.WriteLine($"Convert back: Flag={round.Flag}, Time={round.Time}");
        }
    }

    public class JsonExtensionTestModel
    {

        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class AModel
    {
        public int Id { get; set; }

        [MapName(typeof(AEntity), "Nameyyy")]
        [MapName(typeof(BEntity), "NameB")]
        [MapName("NameDefault")]
        public string Namexx { get; set; } = string.Empty;

        [MapName(typeof(AEntity), "NickY")]
        public string Nick { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    public class AEntity
    {
        public int Id { get; set; }
        public string Nameyyy { get; set; } = string.Empty;
        public string NickY { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class BEntity
    {
        public string NameB { get; set; } = string.Empty;
    }

    public class FallbackEntity
    {
        public string NameDefault { get; set; } = string.Empty;
    }

    public class SelfNameEntity
    {
        public string Nick { get; set; } = string.Empty;
    }

    public class ConvertModel
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Age { get; set; }
        public int Flag { get; set; }
        public long Time { get; set; }
        public DateTime Ignored { get; set; }
    }

    public class ConvertEntity
    {
        public long Id { get; set; }
        public string Amount { get; set; } = string.Empty;
        public int Level { get; set; }
        public ConvertStatus Status { get; set; }
        public int Age { get; set; }
        public bool Flag { get; set; }
        public DateTime Time { get; set; }
        public bool Ignored { get; set; }
    }

    public class ConvertBackModel
    {
        public int Flag { get; set; }
        public long Time { get; set; }
    }

    public enum ConvertStatus
    {
        Unknown = 0,
        Active = 1
    }
}
