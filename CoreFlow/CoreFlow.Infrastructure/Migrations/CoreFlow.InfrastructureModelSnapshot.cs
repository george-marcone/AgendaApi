using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CoreFlow.Infrastructure.Migrations
{
    [DbContext(typeof(CoreFlow.Infrastructure.Data.AppDbContext))]
    partial class CoreFlowInfrastructureModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("CoreFlow.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uniqueidentifier");
                b.Property<string>("Email").HasColumnType("nvarchar(200)").HasMaxLength(200).IsRequired();
                b.Property<string>("Name").HasColumnType("nvarchar(200)").HasMaxLength(200).IsRequired();
                b.Property<string>("Phone").HasColumnType("nvarchar(50)").HasMaxLength(50).IsRequired();
                b.HasKey("Id");
                b.ToTable("Users");
            });
        }
    }
}
