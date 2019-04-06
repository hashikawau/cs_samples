using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FileTransfer.Core.Common
{
    public class HierarchicalExpectedException : ExpectedExceptionBaseAttribute
    {
        private Type[] _expectedExceptionTypes;
        private string _expectedExceptionMessage;

        public HierarchicalExpectedException(params Type[] expectedExceptionTypes)
        {
            _expectedExceptionTypes = expectedExceptionTypes;
            _expectedExceptionMessage = string.Empty;
        }

        //public HierarchyExpectedException(Type expectedExceptionType, string expectedExceptionMessage)
        //{
        //    _expectedExceptionType = expectedExceptionType;
        //    _expectedExceptionMessage = expectedExceptionMessage;
        //}

        protected override void Verify(Exception exception)
        {
            Assert.IsNotNull(exception);

            Exception hierarchy = exception;
            foreach (Type expectedExceptionType in _expectedExceptionTypes)
            {
                Assert.IsInstanceOfType(hierarchy, expectedExceptionType, "Wrong type of exception was thrown.");
                hierarchy = hierarchy.GetBaseException();
            }

            if (!_expectedExceptionMessage.Length.Equals(0))
            {
                Assert.AreEqual(_expectedExceptionMessage, exception.Message, "Wrong exception message was returned.");
            }
        }
    }
}
