namespace DerbyDash.Exceptions {
    public class MissingRacerException : Exception {
        public MissingRacerException() { }

        public MissingRacerException(string message)
            : base(message) { }

        public MissingRacerException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class MissingUserException : Exception {
        public MissingUserException() { }

        public MissingUserException(string message)
            : base(message) { }

        public MissingUserException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class DuplicateRacerException : Exception {
        public DuplicateRacerException() { }

        public DuplicateRacerException(string message)
            : base(message) { }

        public DuplicateRacerException(string message, Exception inner)
            : base(message, inner) { }
    }
}

