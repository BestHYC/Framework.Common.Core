// See https://aka.ms/new-console-template for more information
using ConsoleApp.Test.CKORMTest;
using ConsoleApp.Test.ExtensionTest;
using Framework.Common.Core.ORM;

CKDBContext cKDBContext = new CKDBContext(maxDegreeBulkInsertionOfParallelism: 4, null, null, null) { ViewType = true };

AdgroupCKRepository adgroupCKRepository = new AdgroupCKRepository(new CKExecuteBatch<AdgroupModel>(cKDBContext));
adgroupCKRepository.Where(i => i.AdGroupId > 0).UpdateColumnsWithValue(() => new AdgroupModel() { Name = "123", Rate = 123 }).Update();
adgroupCKRepository.Select(i => new { i.AdGroupId, i.CreationDate, i.Name }).Where(i => i.AdGroupId > 0).QueryList();
//ExtensionTest.RunString();
//ExtensionTest.RunMapper();
Console.WriteLine("Hello, World!");

