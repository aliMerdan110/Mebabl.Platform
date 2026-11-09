CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Carts" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Carts" PRIMARY KEY ("Id")
);

CREATE TABLE "Channels" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Channels" PRIMARY KEY ("Id")
);

CREATE TABLE "Conversations" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Title" character varying(200),
    "IsGroup" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Conversations" PRIMARY KEY ("Id")
);

CREATE TABLE "Developers" (
    "Id" uuid NOT NULL,
    "DisplayName" character varying(200) NOT NULL,
    "Email" character varying(256) NOT NULL,
    "NormalizedEmail" character varying(256) NOT NULL,
    "PasswordHash" character varying(512) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Developers" PRIMARY KEY ("Id")
);

CREATE TABLE "LiveStreams" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(2000),
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_LiveStreams" PRIMARY KEY ("Id")
);

CREATE TABLE "Notifications" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" character varying(100) NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Message" character varying(2000) NOT NULL,
    "Data" jsonb,
    "IsRead" boolean NOT NULL,
    "ReadAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id")
);

CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Status" character varying(32) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
);

CREATE TABLE "Posts" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "OwnerId" uuid NOT NULL,
    "Text" character varying(10000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Posts" PRIMARY KEY ("Id")
);

CREATE TABLE "Products" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "OwnerId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(4000),
    "Price" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "StockQuantity" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
);

CREATE TABLE "SocialComments" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "OwnerId" uuid NOT NULL,
    "ParentCommentId" uuid,
    "Text" character varying(5000) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SocialComments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SocialComments_SocialComments_ParentCommentId" FOREIGN KEY ("ParentCommentId") REFERENCES "SocialComments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SocialReactions" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" character varying(32) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SocialReactions" PRIMARY KEY ("Id")
);

CREATE TABLE "SocialReposts" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SocialReposts" PRIMARY KEY ("Id")
);

CREATE TABLE "SocialShares" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SocialShares" PRIMARY KEY ("Id")
);

CREATE TABLE "Tenants" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Domain" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Tenants" PRIMARY KEY ("Id")
);

CREATE TABLE "RealtimeEvents" (
    "Id" uuid NOT NULL,
    "ChannelId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Payload" jsonb NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_RealtimeEvents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RealtimeEvents_Channels_ChannelId" FOREIGN KEY ("ChannelId") REFERENCES "Channels" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ConversationParticipants" (
    "Id" uuid NOT NULL,
    "ConversationId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "JoinedAt" timestamp with time zone,
    "LeftAt" timestamp with time zone,
    "LastReadAt" timestamp with time zone,
    "IsAdmin" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ConversationParticipants" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ConversationParticipants_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Messages" (
    "Id" uuid NOT NULL,
    "ConversationId" uuid NOT NULL,
    "SenderId" uuid NOT NULL,
    "Content" character varying(10000) NOT NULL,
    "MessageType" character varying(50),
    "IsEdited" boolean NOT NULL,
    "EditedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Messages_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeveloperPasswordResetTokens" (
    "Id" uuid NOT NULL,
    "DeveloperId" uuid NOT NULL,
    "TokenHash" character varying(128) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_DeveloperPasswordResetTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeveloperPasswordResetTokens_Developers_DeveloperId" FOREIGN KEY ("DeveloperId") REFERENCES "Developers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeveloperRefreshTokens" (
    "Id" uuid NOT NULL,
    "DeveloperId" uuid NOT NULL,
    "Token" character varying(512) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_DeveloperRefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeveloperRefreshTokens_Developers_DeveloperId" FOREIGN KEY ("DeveloperId") REFERENCES "Developers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Projects" (
    "Id" uuid NOT NULL,
    "DeveloperId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Description" character varying(2000),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Projects" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Projects_Developers_DeveloperId" FOREIGN KEY ("DeveloperId") REFERENCES "Developers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "LiveStreamSessions" (
    "Id" uuid NOT NULL,
    "LiveStreamId" uuid NOT NULL,
    "PublisherUserId" uuid NOT NULL,
    "PublishTokenHash" character varying(64) NOT NULL,
    "PublishTokenExpiresAt" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "StartedAt" timestamp with time zone,
    "EndedAt" timestamp with time zone,
    CONSTRAINT "PK_LiveStreamSessions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LiveStreamSessions_LiveStreams_LiveStreamId" FOREIGN KEY ("LiveStreamId") REFERENCES "LiveStreams" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StreamCredentials" (
    "Id" uuid NOT NULL,
    "LiveStreamId" uuid NOT NULL,
    "KeyHash" character varying(64) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    CONSTRAINT "PK_StreamCredentials" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_StreamCredentials_LiveStreams_LiveStreamId" FOREIGN KEY ("LiveStreamId") REFERENCES "LiveStreams" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CartItems" (
    "Id" uuid NOT NULL,
    "CartId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CartItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "ProductName" character varying(200) NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "Quantity" integer NOT NULL,
    "TotalPrice" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ProductImages" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "StorageFileId" uuid NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ProductImages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProductImages_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Accounts" (
    "Id" uuid NOT NULL,
    "TenantId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Accounts_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
);

CREATE TABLE "MessageReactions" (
    "Id" uuid NOT NULL,
    "MessageId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Reaction" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_MessageReactions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MessageReactions_Messages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MessageReads" (
    "Id" uuid NOT NULL,
    "MessageId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "ReadAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_MessageReads" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MessageReads_Messages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProjectPlatforms" (
    "Id" uuid NOT NULL,
    "ProjectId" uuid NOT NULL,
    "Platform" character varying(32) NOT NULL,
    "Nickname" character varying(100),
    "PackageName" character varying(255),
    "BundleId" character varying(255),
    "Domain" character varying(255),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ProjectPlatforms" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_ProjectPlatforms_ProjectId_Id" UNIQUE ("ProjectId", "Id"),
    CONSTRAINT "FK_ProjectPlatforms_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProjectSdkConfigurations" (
    "Id" uuid NOT NULL,
    "ProjectId" uuid NOT NULL,
    "ProjectNumber" character varying(10) NOT NULL,
    "PublicKey" character varying(64) NOT NULL,
    "ConfigurationVersion" character varying(20) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ProjectSdkConfigurations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProjectSdkConfigurations_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Profiles" (
    "Id" uuid NOT NULL,
    "AccountId" uuid NOT NULL,
    "Username" character varying(256) NOT NULL,
    "DisplayName" character varying(200) NOT NULL,
    "Bio" character varying(500),
    "AvatarUrl" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Profiles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Profiles_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Applications" (
    "Id" uuid NOT NULL,
    "ProjectId" uuid NOT NULL,
    "ProjectPlatformId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(100) NOT NULL,
    "Description" character varying(2000),
    "IsActive" boolean NOT NULL,
    "TenantId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Applications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Applications_ProjectPlatforms_ProjectId_ProjectPlatformId" FOREIGN KEY ("ProjectId", "ProjectPlatformId") REFERENCES "ProjectPlatforms" ("ProjectId", "Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Applications_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Applications_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
);

CREATE TABLE "ApplicationAuthenticationSettings" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "AllowRegistration" boolean NOT NULL,
    "RequireEmailVerification" boolean NOT NULL,
    "AllowPasswordAuthentication" boolean NOT NULL,
    "AllowAnonymousAuthentication" boolean NOT NULL,
    "PasswordMinLength" integer NOT NULL,
    "SessionLifetimeDays" integer NOT NULL,
    "RefreshTokenLifetimeDays" integer NOT NULL,
    "MaxLoginAttempts" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationAuthenticationSettings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationAuthenticationSettings_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationAuthProviders" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Provider" character varying(50) NOT NULL,
    "IsEnabled" boolean NOT NULL,
    "ConfigurationJson" jsonb,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationAuthProviders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationAuthProviders_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationCredentials" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "ApiKey" character varying(100) NOT NULL,
    "ApiSecretHash" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationCredentials" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationCredentials_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationMobileAppLinks" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "AndroidPackageName" character varying(255),
    "AndroidSha256CertificateFingerprint" character varying(128),
    "IosBundleId" character varying(255),
    "IosTeamId" character varying(64),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ApplicationMobileAppLinks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationMobileAppLinks_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationUsers" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "AccountId" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "NormalizedEmail" character varying(256) NOT NULL,
    "Username" character varying(256) NOT NULL,
    "NormalizedUsername" character varying(256) NOT NULL,
    "PasswordHash" text NOT NULL,
    "SecurityStamp" character varying(256) NOT NULL,
    "EmailConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "AccessFailedCount" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "LastLoginAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationUsers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationUsers_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ApplicationUsers_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Buckets" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Code" character varying(100) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "IsPublic" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Buckets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Buckets_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Collections" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Code" character varying(100) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "IsSystem" boolean NOT NULL DEFAULT FALSE,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Collections" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Collections_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Permissions" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Code" character varying(150) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "Category" character varying(100),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Permissions_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Roles" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Code" character varying(100) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Roles_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationUserEmailVerificationTokens" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "TokenHash" character varying(256) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationUserEmailVerificationTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationUserEmailVerificationTokens_ApplicationUsers_App~" FOREIGN KEY ("ApplicationUserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationUserPasswordResetTokens" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "TokenHash" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationUserPasswordResetTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationUserPasswordResetTokens_ApplicationUsers_Applica~" FOREIGN KEY ("ApplicationUserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "Token" character varying(512) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_ApplicationUsers_ApplicationUserId" FOREIGN KEY ("ApplicationUserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StoredFiles" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "BucketId" uuid,
    "UserId" uuid,
    "Path" character varying(1000) NOT NULL,
    "FileName" character varying(500) NOT NULL,
    "Name" character varying(500) NOT NULL,
    "ContentType" character varying(255) NOT NULL,
    "Extension" character varying(50) NOT NULL,
    "Size" bigint NOT NULL,
    "StorageKey" character varying(2000) NOT NULL,
    "Hash" character varying(128),
    "IsPublic" boolean NOT NULL,
    "CacheControl" character varying(500),
    "ContentDisposition" character varying(500),
    "Version" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_StoredFiles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_StoredFiles_Buckets_BucketId" FOREIGN KEY ("BucketId") REFERENCES "Buckets" ("Id")
);

CREATE TABLE "Documents" (
    "Id" uuid NOT NULL,
    "CollectionId" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "UserId" uuid,
    "Key" character varying(200) NOT NULL,
    "Data" jsonb NOT NULL,
    "Version" integer NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Documents_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SecurityRules" (
    "Id" uuid NOT NULL,
    "CollectionId" uuid NOT NULL,
    "Permission" character varying(100) NOT NULL,
    "CanRead" boolean NOT NULL DEFAULT TRUE,
    "CanWrite" boolean NOT NULL DEFAULT FALSE,
    "CanDelete" boolean NOT NULL DEFAULT FALSE,
    "CanQuery" boolean NOT NULL DEFAULT FALSE,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_SecurityRules" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SecurityRules_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApplicationUserRoles" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ApplicationUserRoles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationUserRoles_ApplicationUsers_ApplicationUserId" FOREIGN KEY ("ApplicationUserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApplicationUserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RolePermissions" (
    "RoleId" uuid NOT NULL,
    "PermissionId" uuid NOT NULL,
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("RoleId", "PermissionId"),
    CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RolePermissions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MessageAttachments" (
    "Id" uuid NOT NULL,
    "MessageId" uuid NOT NULL,
    "StoredFileId" uuid NOT NULL,
    "Caption" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_MessageAttachments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MessageAttachments_Messages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MessageAttachments_StoredFiles_StoredFileId" FOREIGN KEY ("StoredFileId") REFERENCES "StoredFiles" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PostAttachments" (
    "Id" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "StorageFileId" uuid NOT NULL,
    "Type" character varying(32) NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_PostAttachments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PostAttachments_Posts_PostId" FOREIGN KEY ("PostId") REFERENCES "Posts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PostAttachments_StoredFiles_StorageFileId" FOREIGN KEY ("StorageFileId") REFERENCES "StoredFiles" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Accounts_TenantId" ON "Accounts" ("TenantId");

CREATE UNIQUE INDEX "IX_ApplicationAuthenticationSettings_ApplicationId" ON "ApplicationAuthenticationSettings" ("ApplicationId");

CREATE UNIQUE INDEX "IX_ApplicationAuthProviders_ApplicationId_Provider" ON "ApplicationAuthProviders" ("ApplicationId", "Provider");

CREATE UNIQUE INDEX "IX_ApplicationCredentials_ApiKey" ON "ApplicationCredentials" ("ApiKey");

CREATE INDEX "IX_ApplicationCredentials_ApplicationId" ON "ApplicationCredentials" ("ApplicationId");

CREATE INDEX "IX_ApplicationMobileAppLinks_ApplicationId" ON "ApplicationMobileAppLinks" ("ApplicationId");

CREATE UNIQUE INDEX "IX_Applications_ProjectId_Code" ON "Applications" ("ProjectId", "Code");

CREATE UNIQUE INDEX "IX_Applications_ProjectId_ProjectPlatformId" ON "Applications" ("ProjectId", "ProjectPlatformId");

CREATE INDEX "IX_Applications_TenantId" ON "Applications" ("TenantId");

CREATE INDEX "IX_ApplicationUserEmailVerificationTokens_ApplicationUserId_Ex~" ON "ApplicationUserEmailVerificationTokens" ("ApplicationUserId", "ExpiresAt");

CREATE UNIQUE INDEX "IX_ApplicationUserEmailVerificationTokens_TokenHash" ON "ApplicationUserEmailVerificationTokens" ("TokenHash");

CREATE INDEX "IX_ApplicationUserPasswordResetTokens_ApplicationUserId" ON "ApplicationUserPasswordResetTokens" ("ApplicationUserId");

CREATE UNIQUE INDEX "IX_ApplicationUserRoles_ApplicationUserId_RoleId" ON "ApplicationUserRoles" ("ApplicationUserId", "RoleId");

CREATE INDEX "IX_ApplicationUserRoles_RoleId" ON "ApplicationUserRoles" ("RoleId");

CREATE INDEX "IX_ApplicationUsers_AccountId" ON "ApplicationUsers" ("AccountId");

CREATE UNIQUE INDEX "IX_ApplicationUsers_ApplicationId_NormalizedEmail" ON "ApplicationUsers" ("ApplicationId", "NormalizedEmail");

CREATE UNIQUE INDEX "IX_ApplicationUsers_ApplicationId_NormalizedUsername" ON "ApplicationUsers" ("ApplicationId", "NormalizedUsername");

CREATE UNIQUE INDEX "IX_Buckets_ApplicationId_Code" ON "Buckets" ("ApplicationId", "Code");

CREATE UNIQUE INDEX "IX_CartItems_CartId_ProductId" ON "CartItems" ("CartId", "ProductId");

CREATE INDEX "IX_CartItems_ProductId" ON "CartItems" ("ProductId");

CREATE UNIQUE INDEX "IX_Carts_ApplicationId_UserId" ON "Carts" ("ApplicationId", "UserId");

CREATE UNIQUE INDEX "IX_Collections_ApplicationId_Code" ON "Collections" ("ApplicationId", "Code");

CREATE UNIQUE INDEX "IX_Collections_ApplicationId_Name" ON "Collections" ("ApplicationId", "Name");

CREATE UNIQUE INDEX "IX_ConversationParticipants_ConversationId_UserId" ON "ConversationParticipants" ("ConversationId", "UserId");

CREATE INDEX "IX_Conversations_ApplicationId" ON "Conversations" ("ApplicationId");

CREATE INDEX "IX_DeveloperPasswordResetTokens_DeveloperId_ExpiresAt" ON "DeveloperPasswordResetTokens" ("DeveloperId", "ExpiresAt");

CREATE UNIQUE INDEX "IX_DeveloperPasswordResetTokens_TokenHash" ON "DeveloperPasswordResetTokens" ("TokenHash");

CREATE INDEX "IX_DeveloperRefreshTokens_DeveloperId_ExpiresAt" ON "DeveloperRefreshTokens" ("DeveloperId", "ExpiresAt");

CREATE UNIQUE INDEX "IX_DeveloperRefreshTokens_Token" ON "DeveloperRefreshTokens" ("Token");

CREATE UNIQUE INDEX "IX_Developers_NormalizedEmail" ON "Developers" ("NormalizedEmail");

CREATE INDEX "IX_Documents_ApplicationId_CollectionId_CreatedAt" ON "Documents" ("ApplicationId", "CollectionId", "CreatedAt");

CREATE INDEX "IX_Documents_ApplicationId_CollectionId_Key" ON "Documents" ("ApplicationId", "CollectionId", "Key");

CREATE INDEX "IX_Documents_ApplicationId_CollectionId_UserId" ON "Documents" ("ApplicationId", "CollectionId", "UserId");

CREATE INDEX "IX_Documents_CollectionId" ON "Documents" ("CollectionId");

CREATE UNIQUE INDEX "IX_LiveStreams_ApplicationId_Name" ON "LiveStreams" ("ApplicationId", "Name");

CREATE UNIQUE INDEX "IX_LiveStreamSessions_LiveStreamId" ON "LiveStreamSessions" ("LiveStreamId") WHERE "Status" <> 2;

CREATE INDEX "IX_LiveStreamSessions_LiveStreamId_PublisherUserId" ON "LiveStreamSessions" ("LiveStreamId", "PublisherUserId");

CREATE INDEX "IX_LiveStreamSessions_LiveStreamId_Status" ON "LiveStreamSessions" ("LiveStreamId", "Status");

CREATE UNIQUE INDEX "IX_LiveStreamSessions_PublishTokenHash" ON "LiveStreamSessions" ("PublishTokenHash");

CREATE INDEX "IX_MessageAttachments_MessageId" ON "MessageAttachments" ("MessageId");

CREATE UNIQUE INDEX "IX_MessageAttachments_StoredFileId" ON "MessageAttachments" ("StoredFileId");

CREATE UNIQUE INDEX "IX_MessageReactions_MessageId_UserId" ON "MessageReactions" ("MessageId", "UserId");

CREATE UNIQUE INDEX "IX_MessageReads_MessageId_UserId" ON "MessageReads" ("MessageId", "UserId");

CREATE INDEX "IX_Messages_ConversationId_CreatedAt" ON "Messages" ("ConversationId", "CreatedAt");

CREATE INDEX "IX_Notifications_ApplicationId_UserId_IsRead" ON "Notifications" ("ApplicationId", "UserId", "IsRead");

CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");

CREATE INDEX "IX_OrderItems_ProductId" ON "OrderItems" ("ProductId");

CREATE INDEX "IX_Orders_ApplicationId_UserId" ON "Orders" ("ApplicationId", "UserId");

CREATE UNIQUE INDEX "IX_Permissions_ApplicationId_Code" ON "Permissions" ("ApplicationId", "Code");

CREATE INDEX "IX_PostAttachments_PostId_Order" ON "PostAttachments" ("PostId", "Order");

CREATE UNIQUE INDEX "IX_PostAttachments_PostId_StorageFileId" ON "PostAttachments" ("PostId", "StorageFileId");

CREATE INDEX "IX_PostAttachments_StorageFileId" ON "PostAttachments" ("StorageFileId");

CREATE INDEX "IX_Posts_ApplicationId_CreatedAt" ON "Posts" ("ApplicationId", "CreatedAt");

CREATE INDEX "IX_Posts_ApplicationId_OwnerId" ON "Posts" ("ApplicationId", "OwnerId");

CREATE INDEX "IX_ProductImages_ProductId_Order" ON "ProductImages" ("ProductId", "Order");

CREATE INDEX "IX_ProductImages_StorageFileId" ON "ProductImages" ("StorageFileId");

CREATE INDEX "IX_Products_ApplicationId_IsActive" ON "Products" ("ApplicationId", "IsActive");

CREATE UNIQUE INDEX "IX_Profiles_AccountId" ON "Profiles" ("AccountId");

CREATE UNIQUE INDEX "IX_Projects_DeveloperId_Code" ON "Projects" ("DeveloperId", "Code");

CREATE UNIQUE INDEX "IX_ProjectSdkConfigurations_ProjectId" ON "ProjectSdkConfigurations" ("ProjectId");

CREATE UNIQUE INDEX "IX_ProjectSdkConfigurations_ProjectNumber" ON "ProjectSdkConfigurations" ("ProjectNumber");

CREATE UNIQUE INDEX "IX_ProjectSdkConfigurations_PublicKey" ON "ProjectSdkConfigurations" ("PublicKey");

CREATE INDEX "IX_RealtimeEvents_ChannelId" ON "RealtimeEvents" ("ChannelId");

CREATE INDEX "IX_RefreshTokens_ApplicationUserId" ON "RefreshTokens" ("ApplicationUserId");

CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");

CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");

CREATE UNIQUE INDEX "IX_Roles_ApplicationId_Code" ON "Roles" ("ApplicationId", "Code");

CREATE UNIQUE INDEX "IX_SecurityRules_CollectionId_Permission" ON "SecurityRules" ("CollectionId", "Permission");

CREATE INDEX "IX_SocialComments_ApplicationId_OwnerId" ON "SocialComments" ("ApplicationId", "OwnerId");

CREATE INDEX "IX_SocialComments_ApplicationId_PostId_CreatedAt" ON "SocialComments" ("ApplicationId", "PostId", "CreatedAt");

CREATE INDEX "IX_SocialComments_ParentCommentId" ON "SocialComments" ("ParentCommentId");

CREATE INDEX "IX_SocialReactions_ApplicationId_PostId" ON "SocialReactions" ("ApplicationId", "PostId");

CREATE UNIQUE INDEX "IX_SocialReactions_ApplicationId_PostId_UserId" ON "SocialReactions" ("ApplicationId", "PostId", "UserId");

CREATE INDEX "IX_SocialReposts_ApplicationId_PostId" ON "SocialReposts" ("ApplicationId", "PostId");

CREATE UNIQUE INDEX "IX_SocialReposts_ApplicationId_PostId_UserId" ON "SocialReposts" ("ApplicationId", "PostId", "UserId");

CREATE INDEX "IX_SocialShares_ApplicationId_PostId" ON "SocialShares" ("ApplicationId", "PostId");

CREATE UNIQUE INDEX "IX_SocialShares_ApplicationId_PostId_UserId" ON "SocialShares" ("ApplicationId", "PostId", "UserId");

CREATE UNIQUE INDEX "IX_StoredFiles_ApplicationId_StorageKey" ON "StoredFiles" ("ApplicationId", "StorageKey");

CREATE INDEX "IX_StoredFiles_ApplicationId_UserId_Path" ON "StoredFiles" ("ApplicationId", "UserId", "Path");

CREATE INDEX "IX_StoredFiles_BucketId" ON "StoredFiles" ("BucketId");

CREATE UNIQUE INDEX "IX_StreamCredentials_KeyHash" ON "StreamCredentials" ("KeyHash");

CREATE INDEX "IX_StreamCredentials_LiveStreamId" ON "StreamCredentials" ("LiveStreamId");

CREATE UNIQUE INDEX "IX_Tenants_Code" ON "Tenants" ("Code");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261020124719_InitialCreate', '10.0.10');

COMMIT;

