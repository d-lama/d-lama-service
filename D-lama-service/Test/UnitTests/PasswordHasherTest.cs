using d_lama_service.Models;

namespace MSTest.UnitTests
{
    [TestClass]
    public class PasswordHasherTest
    {
        [TestMethod]
        public void ComputeHashTest()
        {
            string password = null;
            string salt = null;
            string pepper = null;
            int iteration = 0;

            // nulls, 0 iteration
            string hash = PasswordHasher.ComputeHash(password, salt, pepper, iteration);
            Assert.IsNull(hash);

            // nulls, 1 iteration
            iteration = 1;
            hash = PasswordHasher.ComputeHash(password, salt, pepper, iteration);
            Assert.IsNotNull(hash);
            Assert.AreNotEqual(password, hash);

            // nulls, 2 iteration
            iteration = 2;
            string hash2 = PasswordHasher.ComputeHash(password, salt, pepper, iteration);
            Assert.IsNotNull(hash2);
            Assert.AreNotEqual(password, hash2);
            Assert.AreNotEqual(hash, hash2); // hash not same as before

            // normal values, 2 iteration
            password = "Test123";
            salt = "mySalt";
            pepper = "myPepper";
            string hash3 = PasswordHasher.ComputeHash(password, salt, pepper, iteration);
            Assert.IsNotNull(hash3);
            Assert.AreNotEqual(password, hash3);
            Assert.AreNotEqual(hash, hash3);
        }


        [TestMethod]
        public void GenerateSaltTest()
        {
            string salt1 = PasswordHasher.GenerateSalt();
            string salt2 = PasswordHasher.GenerateSalt();

            Assert.AreNotEqual(salt1, salt2);
        }
    }
}