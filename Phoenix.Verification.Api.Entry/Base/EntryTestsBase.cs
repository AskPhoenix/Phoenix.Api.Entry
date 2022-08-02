namespace Phoenix.Verification.Api.Entry.Base
{
    public class EntryTestsBase : AuthenticatedTestsBase
    {
        protected readonly string API_BASE;

        public EntryTestsBase()
            : base()
        {
            API_BASE = _configuration["Api:BaseUri"];
        }
    }
}
