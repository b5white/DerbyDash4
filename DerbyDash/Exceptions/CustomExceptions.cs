namespace DerbyDash.Exceptions {
    public class MissingFamilyMemberException: Exception {
        public MissingFamilyMemberException() { }

        public MissingFamilyMemberException(string message)
            : base(message) { }

        public MissingFamilyMemberException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class MissingUserException: Exception {
        public MissingUserException() { }

        public MissingUserException(string message)
            : base(message) { }

        public MissingUserException(string message, Exception inner)
            : base(message, inner) { }
    }
}
