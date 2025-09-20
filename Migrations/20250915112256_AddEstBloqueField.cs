using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEstBloqueField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantTelemedecine_Telemedecine_TelemedicineId",
                table: "ParticipantTelemedecine");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantTelemedecine_Utilisateur_UtilisateurId",
                table: "ParticipantTelemedecine");

            migrationBuilder.DropForeignKey(
                name: "FK_Telemedecine_Utilisateur_CreateurId",
                table: "Telemedecine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Telemedecine",
                table: "Telemedecine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParticipantTelemedecine",
                table: "ParticipantTelemedecine");

            migrationBuilder.RenameTable(
                name: "Telemedecine",
                newName: "Telemedecines");

            migrationBuilder.RenameTable(
                name: "ParticipantTelemedecine",
                newName: "ParticipantsTelemedecine");

            migrationBuilder.RenameIndex(
                name: "IX_Telemedecine_CreateurId",
                table: "Telemedecines",
                newName: "IX_Telemedecines_CreateurId");

            migrationBuilder.RenameIndex(
                name: "IX_ParticipantTelemedecine_UtilisateurId",
                table: "ParticipantsTelemedecine",
                newName: "IX_ParticipantsTelemedecine_UtilisateurId");

            migrationBuilder.RenameIndex(
                name: "IX_ParticipantTelemedecine_TelemedicineId",
                table: "ParticipantsTelemedecine",
                newName: "IX_ParticipantsTelemedecine_TelemedicineId");

            migrationBuilder.AddColumn<bool>(
                name: "EstBloque",
                table: "Utilisateur",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isOnline",
                table: "Utilisateur",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Telemedecines",
                table: "Telemedecines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParticipantsTelemedecine",
                table: "ParticipantsTelemedecine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantsTelemedecine_Telemedecines_TelemedicineId",
                table: "ParticipantsTelemedecine",
                column: "TelemedicineId",
                principalTable: "Telemedecines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantsTelemedecine_Utilisateur_UtilisateurId",
                table: "ParticipantsTelemedecine",
                column: "UtilisateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Telemedecines_Utilisateur_CreateurId",
                table: "Telemedecines",
                column: "CreateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantsTelemedecine_Telemedecines_TelemedicineId",
                table: "ParticipantsTelemedecine");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantsTelemedecine_Utilisateur_UtilisateurId",
                table: "ParticipantsTelemedecine");

            migrationBuilder.DropForeignKey(
                name: "FK_Telemedecines_Utilisateur_CreateurId",
                table: "Telemedecines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Telemedecines",
                table: "Telemedecines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParticipantsTelemedecine",
                table: "ParticipantsTelemedecine");

            migrationBuilder.DropColumn(
                name: "EstBloque",
                table: "Utilisateur");

            migrationBuilder.DropColumn(
                name: "isOnline",
                table: "Utilisateur");

            migrationBuilder.RenameTable(
                name: "Telemedecines",
                newName: "Telemedecine");

            migrationBuilder.RenameTable(
                name: "ParticipantsTelemedecine",
                newName: "ParticipantTelemedecine");

            migrationBuilder.RenameIndex(
                name: "IX_Telemedecines_CreateurId",
                table: "Telemedecine",
                newName: "IX_Telemedecine_CreateurId");

            migrationBuilder.RenameIndex(
                name: "IX_ParticipantsTelemedecine_UtilisateurId",
                table: "ParticipantTelemedecine",
                newName: "IX_ParticipantTelemedecine_UtilisateurId");

            migrationBuilder.RenameIndex(
                name: "IX_ParticipantsTelemedecine_TelemedicineId",
                table: "ParticipantTelemedecine",
                newName: "IX_ParticipantTelemedecine_TelemedicineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Telemedecine",
                table: "Telemedecine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParticipantTelemedecine",
                table: "ParticipantTelemedecine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantTelemedecine_Telemedecine_TelemedicineId",
                table: "ParticipantTelemedecine",
                column: "TelemedicineId",
                principalTable: "Telemedecine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantTelemedecine_Utilisateur_UtilisateurId",
                table: "ParticipantTelemedecine",
                column: "UtilisateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Telemedecine_Utilisateur_CreateurId",
                table: "Telemedecine",
                column: "CreateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
