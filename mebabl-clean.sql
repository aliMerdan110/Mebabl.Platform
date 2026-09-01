CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "BlockedUsers" (
    "Id" uuid NOT NULL,
    "BlockerId" uuid NOT NULL,
    "BlockedId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BlockedUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "Carts" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Carts" PRIMARY KEY ("Id")
);

CREATE TABLE "Categories" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "ParentId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);

CREATE TABLE "Comments" (
    "Id" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "Content" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Comments" PRIMARY KEY ("Id")
);

CREATE TABLE "ConversationParticipants" (
    "Id" uuid NOT NULL,
    "ConversationId" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "JoinedAt" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_ConversationParticipants" PRIMARY KEY ("Id")
);

CREATE TABLE "Conversations" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Conversations" PRIMARY KEY ("Id")
);

CREATE TABLE "Coupons" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Code" text NOT NULL,
    "DiscountAmount" numeric NOT NULL,
    "IsPercentage" boolean NOT NULL,
    "ExpiryDate" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id")
);

CREATE TABLE "Developers" (
    "Id" uuid NOT NULL,
    "DisplayName" character varying(200) NOT NULL,
    "Email" character varying(256) NOT NULL,
    "NormalizedEmail" character varying(256) NOT NULL,
    "PasswordHash" text NOT NULL,
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

CREATE TABLE "Follows" (
    "Id" uuid NOT NULL,
    "FollowerId" uuid NOT NULL,
    "FollowingId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Follows" PRIMARY KEY ("Id")
);

CREATE TABLE "Likes" (
    "Id" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Likes" PRIMARY KEY ("Id")
);

CREATE TABLE "Medias" (
    "Id" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "Url" text NOT NULL,
    "Type" text NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Medias" PRIMARY KEY ("Id")
);

CREATE TABLE "Mentions" (
    "Id" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "MentionedUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Mentions" PRIMARY KEY ("Id")
);

CREATE TABLE "MessageAttachments" (
    "Id" uuid NOT NULL,
    "MessageId" uuid NOT NULL,
    "Url" text NOT NULL,
    "Type" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MessageAttachments" PRIMARY KEY ("Id")
);

CREATE TABLE "MessageReactions" (
    "Id" uuid NOT NULL,
    "MessageId" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "Reaction" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MessageReactions" PRIMARY KEY ("Id")
);

CREATE TABLE "Messages" (
    "Id" uuid NOT NULL,
    "ConversationId" uuid NOT NULL,
    "SenderId" uuid NOT NULL,
    "Content" text NOT NULL,
    "IsRead" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("Id")
);

CREATE TABLE "Notifications" (
    "Id" uuid NOT NULL,
    "ReceiverId" uuid NOT NULL,
    "SenderId" uuid,
    "Type" text NOT NULL,
    "Message" text NOT NULL,
    "IsRead" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id")
);

CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "TotalAmount" numeric NOT NULL,
    "Status" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
);

CREATE TABLE "Payments" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "Amount" numeric NOT NULL,
    "Method" text NOT NULL,
    "Status" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
);

CREATE TABLE "Posts" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "Content" text NOT NULL,
    "IsPublished" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Posts" PRIMARY KEY ("Id")
);

CREATE TABLE "Products" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(2000) NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
);

CREATE TABLE "Reviews" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "Rating" integer NOT NULL,
    "Comment" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id")
);

CREATE TABLE "Shares" (
    "Id" uuid NOT NULL,
    "PostId" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Shares" PRIMARY KEY ("Id")
);

CREATE TABLE "Shipments" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "Address" text NOT NULL,
    "TrackingNumber" text NOT NULL,
    "Status" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Shipments" PRIMARY KEY ("Id")
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

CREATE TABLE "Wishlists" (
    "Id" uuid NOT NULL,
    "ApplicationUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Wishlists" PRIMARY KEY ("Id")
);

CREATE TABLE "CartItems" (
    "Id" uuid NOT NULL,
    "CartId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "Price" numeric NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CartItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Inventories" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "ReservedQuantity" integer NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Inventories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Inventories_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE TABLE "OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ProductMedias" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Url" character varying(1000) NOT NULL,
    "Type" text NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ProductMedias" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProductMedias_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Accounts" (
    "Id" uuid NOT NULL,
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

CREATE TABLE "Applications" (
    "Id" uuid NOT NULL,
    "DeveloperId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "ApiKey" text NOT NULL,
    "ApiSecret" text NOT NULL,
    "Description" text,
    "Domain" text,
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
    CONSTRAINT "FK_Applications_Developers_DeveloperId" FOREIGN KEY ("DeveloperId") REFERENCES "Developers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Applications_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
);

CREATE TABLE "WishlistItems" (
    "Id" uuid NOT NULL,
    "WishlistId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_WishlistItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WishlistItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_WishlistItems_Wishlists_WishlistId" FOREIGN KEY ("WishlistId") REFERENCES "Wishlists" ("Id") ON DELETE CASCADE
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

CREATE TABLE "ApplicationUsers" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "AccountId" uuid NOT NULL,
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
    CONSTRAINT "FK_ApplicationUsers_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApplicationUsers_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Collections" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500) NOT NULL,
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
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    "Category" text,
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
    "Description" text NOT NULL,
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

CREATE TABLE "Documents" (
    "Id" uuid NOT NULL,
    "CollectionId" uuid NOT NULL,
    "Key" character varying(200) NOT NULL,
    "Data" jsonb NOT NULL,
    "Version" integer NOT NULL DEFAULT 1,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Documents_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE
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

CREATE UNIQUE INDEX "IX_Accounts_NormalizedEmail" ON "Accounts" ("NormalizedEmail");

CREATE UNIQUE INDEX "IX_Accounts_NormalizedUsername" ON "Accounts" ("NormalizedUsername");

CREATE INDEX "IX_Accounts_TenantId" ON "Accounts" ("TenantId");

CREATE UNIQUE INDEX "IX_Applications_Code" ON "Applications" ("Code");

CREATE INDEX "IX_Applications_DeveloperId" ON "Applications" ("DeveloperId");

CREATE INDEX "IX_Applications_TenantId" ON "Applications" ("TenantId");

CREATE UNIQUE INDEX "IX_ApplicationUserRoles_ApplicationUserId_RoleId" ON "ApplicationUserRoles" ("ApplicationUserId", "RoleId");

CREATE INDEX "IX_ApplicationUserRoles_RoleId" ON "ApplicationUserRoles" ("RoleId");

CREATE UNIQUE INDEX "IX_ApplicationUsers_AccountId_ApplicationId" ON "ApplicationUsers" ("AccountId", "ApplicationId");

CREATE INDEX "IX_ApplicationUsers_ApplicationId" ON "ApplicationUsers" ("ApplicationId");

CREATE INDEX "IX_CartItems_CartId" ON "CartItems" ("CartId");

CREATE INDEX "IX_CartItems_ProductId" ON "CartItems" ("ProductId");

CREATE UNIQUE INDEX "IX_Collections_ApplicationId_Name" ON "Collections" ("ApplicationId", "Name");

CREATE UNIQUE INDEX "IX_Developers_NormalizedEmail" ON "Developers" ("NormalizedEmail");

CREATE UNIQUE INDEX "IX_Documents_CollectionId_Key" ON "Documents" ("CollectionId", "Key");

CREATE INDEX "IX_Inventories_ProductId" ON "Inventories" ("ProductId");

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");

CREATE INDEX "IX_OrderItems_ProductId" ON "OrderItems" ("ProductId");

CREATE INDEX "IX_Permissions_ApplicationId" ON "Permissions" ("ApplicationId");

CREATE INDEX "IX_ProductMedias_ProductId" ON "ProductMedias" ("ProductId");

CREATE UNIQUE INDEX "IX_Profiles_AccountId" ON "Profiles" ("AccountId");

CREATE INDEX "IX_RefreshTokens_ApplicationUserId" ON "RefreshTokens" ("ApplicationUserId");

CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");

CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");

CREATE INDEX "IX_Roles_ApplicationId" ON "Roles" ("ApplicationId");

CREATE UNIQUE INDEX "IX_Tenants_Code" ON "Tenants" ("Code");

CREATE INDEX "IX_WishlistItems_ProductId" ON "WishlistItems" ("ProductId");

CREATE INDEX "IX_WishlistItems_WishlistId" ON "WishlistItems" ("WishlistId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260802140025_InitialCreate', '10.0.10');

COMMIT;

START TRANSACTION;
ALTER TABLE "Applications" ALTER COLUMN "ApiSecret" TYPE character varying(200);

ALTER TABLE "Applications" ALTER COLUMN "ApiKey" TYPE character varying(100);

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

CREATE UNIQUE INDEX "IX_Applications_ApiKey" ON "Applications" ("ApiKey");

CREATE UNIQUE INDEX "IX_ApplicationCredentials_ApiKey" ON "ApplicationCredentials" ("ApiKey");

CREATE UNIQUE INDEX "IX_ApplicationCredentials_ApplicationId" ON "ApplicationCredentials" ("ApplicationId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260802223541_AddApplicationCredentials', '10.0.10');

COMMIT;

START TRANSACTION;
DROP INDEX "IX_Applications_ApiKey";

DROP INDEX "IX_ApplicationCredentials_ApplicationId";

ALTER TABLE "Applications" DROP COLUMN "ApiKey";

ALTER TABLE "Applications" DROP COLUMN "ApiSecret";

CREATE TABLE "DeveloperRefreshTokens" (
    "Id" uuid NOT NULL,
    "DeveloperId" uuid NOT NULL,
    "Token" text NOT NULL,
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

CREATE INDEX "IX_ApplicationCredentials_ApplicationId" ON "ApplicationCredentials" ("ApplicationId");

CREATE INDEX "IX_DeveloperRefreshTokens_DeveloperId" ON "DeveloperRefreshTokens" ("DeveloperId");

CREATE UNIQUE INDEX "IX_DeveloperRefreshTokens_Token" ON "DeveloperRefreshTokens" ("Token");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260803101526_MoveApplicationCredentials', '10.0.10');

COMMIT;

START TRANSACTION;
ALTER TABLE "Roles" ADD "Code" text NOT NULL DEFAULT '';

ALTER TABLE "Collections" ADD "Code" character varying(100) NOT NULL DEFAULT '';

ALTER TABLE "Collections" ADD "IsSystem" boolean NOT NULL DEFAULT FALSE;

CREATE UNIQUE INDEX "IX_Collections_ApplicationId_Code" ON "Collections" ("ApplicationId", "Code");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260804103138_UpdateCollectionSchema', '10.0.10');

COMMIT;

START TRANSACTION;
ALTER TABLE "Documents" ALTER COLUMN "IsDeleted" SET DEFAULT FALSE;

ALTER TABLE "Documents" ADD "ETag" character varying(100);

CREATE INDEX "IX_Documents_CollectionId_IsDeleted" ON "Documents" ("CollectionId", "IsDeleted");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260804103504_UpgradeDocumentEngine', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE UNIQUE INDEX "IX_SecurityRules_CollectionId_Permission" ON "SecurityRules" ("CollectionId", "Permission");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260804105245_AddSecurityRules', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE TABLE "StoredFiles" (
    "Id" uuid NOT NULL,
    "BucketId" uuid NOT NULL,
    "Key" character varying(200) NOT NULL,
    "FileName" character varying(300) NOT NULL,
    "ContentType" character varying(150) NOT NULL,
    "Extension" character varying(20) NOT NULL,
    "Size" bigint NOT NULL,
    "Hash" character varying(128) NOT NULL,
    "StoragePath" character varying(1000) NOT NULL,
    "Metadata" jsonb NOT NULL,
    "Version" integer NOT NULL DEFAULT 1,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "UpdatedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" uuid,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_StoredFiles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_StoredFiles_Buckets_BucketId" FOREIGN KEY ("BucketId") REFERENCES "Buckets" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Buckets_ApplicationId_Code" ON "Buckets" ("ApplicationId", "Code");

CREATE UNIQUE INDEX "IX_StoredFiles_BucketId_Key" ON "StoredFiles" ("BucketId", "Key");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807111347_AddStorageService', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE INDEX "IX_RealtimeEvents_ChannelId" ON "RealtimeEvents" ("ChannelId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807132823_AddRealtimeService', '10.0.10');

COMMIT;

START TRANSACTION;
DROP TABLE "BlockedUsers";

DROP TABLE "CartItems";

DROP TABLE "Categories";

DROP TABLE "Comments";

DROP TABLE "ConversationParticipants";

DROP TABLE "Conversations";

DROP TABLE "Coupons";

DROP TABLE "Follows";

DROP TABLE "Inventories";

DROP TABLE "Likes";

DROP TABLE "Medias";

DROP TABLE "Mentions";

DROP TABLE "MessageAttachments";

DROP TABLE "MessageReactions";

DROP TABLE "Messages";

DROP TABLE "OrderItems";

DROP TABLE "Payments";

DROP TABLE "Posts";

DROP TABLE "ProductMedias";

DROP TABLE "Reviews";

DROP TABLE "Shares";

DROP TABLE "Shipments";

DROP TABLE "WishlistItems";

DROP TABLE "Carts";

DROP TABLE "Orders";

DROP TABLE "Products";

DROP TABLE "Wishlists";

ALTER TABLE "Notifications" RENAME COLUMN "SenderId" TO "UpdatedBy";

ALTER TABLE "Notifications" RENAME COLUMN "ReceiverId" TO "UserId";

ALTER TABLE "Notifications" ALTER COLUMN "Type" TYPE character varying(100);

ALTER TABLE "Notifications" ALTER COLUMN "Message" TYPE character varying(2000);

ALTER TABLE "Notifications" ADD "ApplicationId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "Notifications" ADD "CreatedBy" uuid;

ALTER TABLE "Notifications" ADD "Data" jsonb;

ALTER TABLE "Notifications" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "Notifications" ADD "DeletedBy" uuid;

ALTER TABLE "Notifications" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Notifications" ADD "ReadAt" timestamp with time zone;

ALTER TABLE "Notifications" ADD "Title" character varying(200) NOT NULL DEFAULT '';

ALTER TABLE "Notifications" ADD "UpdatedAt" timestamp with time zone;

CREATE INDEX "IX_Notifications_ApplicationId_UserId_IsRead" ON "Notifications" ("ApplicationId", "UserId", "IsRead");

CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260808131619_RemoveLegacyModulesAndUpgradeNotifications', '10.0.10');

COMMIT;

START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260808131911_AddNotificationsService', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE UNIQUE INDEX "IX_ConversationParticipants_ConversationId_UserId" ON "ConversationParticipants" ("ConversationId", "UserId");

CREATE INDEX "IX_Conversations_ApplicationId" ON "Conversations" ("ApplicationId");

CREATE INDEX "IX_Messages_ConversationId_CreatedAt" ON "Messages" ("ConversationId", "CreatedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260808135855_AddChatService', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE UNIQUE INDEX "IX_MessageReads_MessageId_UserId" ON "MessageReads" ("MessageId", "UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260808141541_AddMessageRead', '10.0.10');

COMMIT;

START TRANSACTION;
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

CREATE INDEX "IX_MessageAttachments_MessageId" ON "MessageAttachments" ("MessageId");

CREATE UNIQUE INDEX "IX_MessageAttachments_StoredFileId" ON "MessageAttachments" ("StoredFileId");

CREATE UNIQUE INDEX "IX_MessageReactions_MessageId_UserId" ON "MessageReactions" ("MessageId", "UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260808144322_AddChatAttachments', '10.0.10');

COMMIT;

START TRANSACTION;
CREATE TABLE "DeveloperPasswordResetToken" (
    "Id" uuid NOT NULL,
    "DeveloperId" uuid NOT NULL,
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
    CONSTRAINT "PK_DeveloperPasswordResetToken" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeveloperPasswordResetToken_Developers_DeveloperId" FOREIGN KEY ("DeveloperId") REFERENCES "Developers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_DeveloperPasswordResetToken_DeveloperId" ON "DeveloperPasswordResetToken" ("DeveloperId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260812015709_AddDeveloperPasswordResetTokens', '10.0.10');

COMMIT;

START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260812015856_Add2DeveloperPasswordResetTokens', '10.0.10');

COMMIT;

START TRANSACTION;
ALTER TABLE "DeveloperPasswordResetToken" DROP CONSTRAINT "FK_DeveloperPasswordResetToken_Developers_DeveloperId";

ALTER TABLE "DeveloperPasswordResetToken" DROP CONSTRAINT "PK_DeveloperPasswordResetToken";

DROP INDEX "IX_DeveloperPasswordResetToken_DeveloperId";

ALTER TABLE "DeveloperPasswordResetToken" RENAME TO "DeveloperPasswordResetTokens";

ALTER TABLE "DeveloperPasswordResetTokens" ALTER COLUMN "TokenHash" TYPE character varying(128);

ALTER TABLE "DeveloperPasswordResetTokens" ADD CONSTRAINT "PK_DeveloperPasswordResetTokens" PRIMARY KEY ("Id");

CREATE INDEX "IX_DeveloperPasswordResetTokens_DeveloperId_ExpiresAt" ON "DeveloperPasswordResetTokens" ("DeveloperId", "ExpiresAt");

CREATE UNIQUE INDEX "IX_DeveloperPasswordResetTokens_TokenHash" ON "DeveloperPasswordResetTokens" ("TokenHash");

ALTER TABLE "DeveloperPasswordResetTokens" ADD CONSTRAINT "FK_DeveloperPasswordResetTokens_Developers_DeveloperId" FOREIGN KEY ("DeveloperId") REFERENCES "Developers" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260812020321_Add3DeveloperPasswordResetTokens', '10.0.10');

COMMIT;

