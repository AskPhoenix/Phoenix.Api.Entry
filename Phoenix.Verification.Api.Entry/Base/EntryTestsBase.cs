namespace Phoenix.Verification.Api.Entry.Base
{
    public class EntryTestsBase : AuthenticatedTestsBase
    {
        // TODO: Create tests for all controllers

        protected readonly string API_BASE;

        public EntryTestsBase()
            : base()
        {
            API_BASE = _configuration["Api:BaseUri"];
        }
    }
}
