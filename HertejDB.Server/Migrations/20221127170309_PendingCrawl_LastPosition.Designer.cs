﻿// <auto-generated />
using System;
using HertejDB.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HertejDB.Server.Migrations
{
    [DbContext(typeof(HertejDbContext))]
    [Migration("20221127170309_PendingCrawl_LastPosition")]
    partial class PendingCrawlLastPosition
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("HertejDB.Server.Data.Image", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("Added")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RatingStatus")
                        .HasColumnType("integer");

                    b.Property<string>("StorageId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Category");

                    b.HasIndex("RatingStatus");

                    b.HasIndex("RatingStatus", "Category");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("HertejDB.Server.Data.ImageRating", b =>
                {
                    b.Property<long>("ImageId")
                        .HasColumnType("bigint");

                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<bool>("IsSuitable")
                        .HasColumnType("boolean");

                    b.HasKey("ImageId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ImageRatings");
                });

            modelBuilder.Entity("HertejDB.Server.Data.PendingCrawl", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DesiredCount")
                        .HasColumnType("integer");

                    b.Property<string>("LastPosition")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("MaxAtOnce")
                        .HasColumnType("integer");

                    b.Property<string>("SearchParameter")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PendingCrawls");
                });

            modelBuilder.Entity("HertejDB.Server.Data.Image", b =>
                {
                    b.OwnsOne("HertejDB.Common.ImageSourceAttribution", "SourceAttribution", b1 =>
                        {
                            b1.Property<long>("ImageId")
                                .HasColumnType("bigint");

                            b1.Property<string>("Author")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<DateTimeOffset>("Creation")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("License")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("RemoteId")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("RemoteUrl")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("SourceName")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("ImageId");

                            b1.ToTable("Images");

                            b1.WithOwner()
                                .HasForeignKey("ImageId");
                        });

                    b.Navigation("SourceAttribution");
                });

            modelBuilder.Entity("HertejDB.Server.Data.ImageRating", b =>
                {
                    b.HasOne("HertejDB.Server.Data.Image", "Image")
                        .WithMany("Ratings")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");
                });

            modelBuilder.Entity("HertejDB.Server.Data.Image", b =>
                {
                    b.Navigation("Ratings");
                });
#pragma warning restore 612, 618
        }
    }
}
