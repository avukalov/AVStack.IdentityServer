namespace AVStack.IdentityServer.WebApi.Common.Constants
{
    public static class IdentityClaimDefaults
    {
        public const string FullName = "full_name";
        public const string HasuraPath = "https://hasura.io/jwt/claims";
        public const string HasuraAllowedRoles = "x-hasura-allowed-roles";
        public const string HasuraDefaultRole = "x-hasura-default-role";
        public const string HasuraRole = "x-hasura-role";
        public const string HasuraUserId = "x-hasura-user-id";
        public const string HasuraOrganizationId = "x-hasura-org-id";
    }
}