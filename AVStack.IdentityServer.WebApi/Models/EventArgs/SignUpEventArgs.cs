namespace AVStack.IdentityServer.WebApi.Models.EventArgs
{
    public class SignUpEventArgs : global::System.EventArgs
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Callback { get; set; }
    }
}