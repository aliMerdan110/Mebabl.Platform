using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebabl.Platform.Infrastructure.Migrations
{
    public partial class AddLivePermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    app RECORD;
                    owner_role_id uuid;
                    live_view_id uuid;
                    live_publish_id uuid;
                BEGIN
                    FOR app IN
                        SELECT "Id"
                        FROM "Applications"
                    LOOP
                        SELECT "Id"
                        INTO owner_role_id
                        FROM "Roles"
                        WHERE "ApplicationId" = app."Id"
                          AND "Name" = 'Owner'
                        LIMIT 1;

                        IF owner_role_id IS NULL THEN
                            CONTINUE;
                        END IF;

                        SELECT "Id"
                        INTO live_view_id
                        FROM "Permissions"
                        WHERE "ApplicationId" = app."Id"
                          AND LOWER("Code") = 'live.view'
                        LIMIT 1;

                        IF live_view_id IS NULL THEN
                            live_view_id := gen_random_uuid();

                            INSERT INTO "Permissions"
                            (
                                "Id",
                                "ApplicationId",
                                "Name",
                                "Code",
                                "Description",
                                "IsActive",
                                "IsDeleted",
                                "CreatedAt",
                                "UpdatedAt"
                            )
                            VALUES
                            (
                                live_view_id,
                                app."Id",
                                'live.view',
                                'live.view',
                                'live.view',
                                TRUE,
                                FALSE,
                                NOW(),
                                NOW()
                            );
                        END IF;

                        SELECT "Id"
                        INTO live_publish_id
                        FROM "Permissions"
                        WHERE "ApplicationId" = app."Id"
                          AND LOWER("Code") = 'live.publish'
                        LIMIT 1;

                        IF live_publish_id IS NULL THEN
                            live_publish_id := gen_random_uuid();

                            INSERT INTO "Permissions"
                            (
                                "Id",
                                "ApplicationId",
                                "Name",
                                "Code",
                                "Description",
                                "IsActive",
                                "IsDeleted",
                                "CreatedAt",
                                "UpdatedAt"
                            )
                            VALUES
                            (
                                live_publish_id,
                                app."Id",
                                'live.publish',
                                'live.publish',
                                'live.publish',
                                TRUE,
                                FALSE,
                                NOW(),
                                NOW()
                            );
                        END IF;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM "RolePermissions"
                            WHERE "RoleId" = owner_role_id
                              AND "PermissionId" = live_view_id
                        )
                        THEN
                            INSERT INTO "RolePermissions"
                            (
                                "RoleId",
                                "PermissionId"
                            )
                            VALUES
                            (
                                owner_role_id,
                                live_view_id
                            );
                        END IF;

                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM "RolePermissions"
                            WHERE "RoleId" = owner_role_id
                              AND "PermissionId" = live_publish_id
                        )
                        THEN
                            INSERT INTO "RolePermissions"
                            (
                                "RoleId",
                                "PermissionId"
                            )
                            VALUES
                            (
                                owner_role_id,
                                live_publish_id
                            );
                        END IF;
                    END LOOP;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "RolePermissions" rp
                USING "Permissions" p
                WHERE rp."PermissionId" = p."Id"
                  AND LOWER(p."Code") IN ('live.view', 'live.publish');

                DELETE FROM "Permissions"
                WHERE LOWER("Code") IN ('live.view', 'live.publish');
                """);
        }
    }
}