﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NiBot.Kahla;

namespace NiBot.Kahla.Migrations
{
    [DbContext(typeof(NiDbContext))]
    partial class NiDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("NiBot.Kahla.Models.NiBind", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Command")
                        .HasColumnType("TEXT");

                    b.Property<int>("ConversationId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<int>("Mode")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Binds");
                });
#pragma warning restore 612, 618
        }
    }
}
