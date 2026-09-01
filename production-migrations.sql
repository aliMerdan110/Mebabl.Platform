START TRANSACTION;
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

CREATE TABLE "ApplicationUserPasswordResetTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
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
    CONSTRAINT "FK_ApplicationUserPasswordResetTokens_ApplicationUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_ApplicationAuthProviders_ApplicationId_Provider" ON "ApplicationAuthProviders" ("ApplicationId", "Provider");

CREATE INDEX "IX_ApplicationUserPasswordResetTokens_UserId" ON "ApplicationUserPasswordResetTokens" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260819152448_AddApplicationAuthProviders', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE UNIQUE INDEX "IX_ApplicationAuthenticationSettings_ApplicationId" ON "ApplicationAuthenticationSettings" ("ApplicationId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260821010521_AddApplicationAuthenticationSettings', '10.0.10');

COMMIT;

START TRANSACTION;
CREATE TABLE "ApplicationPlatforms" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
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
    CONSTRAINT "PK_ApplicationPlatforms" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationPlatforms_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_ApplicationPlatforms_ApplicationId_Platform" ON "ApplicationPlatforms" ("ApplicationId", "Platform");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260824200601_AddApplicationPlatforms', '10.0.10');

COMMIT;

