﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SQL;

namespace SQL.Migrations
{
    [DbContext(typeof(LibraryContext))]
    partial class LibraryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ImageClassImageInfo", b =>
                {
                    b.Property<int>("ImageClassesImageClassId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ImagesImageInfoId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ImageClassesImageClassId", "ImagesImageInfoId");

                    b.HasIndex("ImagesImageInfoId");

                    b.ToTable("ImageClassImageInfo");
                });

            modelBuilder.Entity("SQL.ImageClass", b =>
                {
                    b.Property<int>("ImageClassId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClassName")
                        .HasColumnType("TEXT");

                    b.HasKey("ImageClassId");

                    b.ToTable("ImageClasses");
                });

            modelBuilder.Entity("SQL.ImageFile", b =>
                {
                    b.Property<int>("ImageFileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Img")
                        .HasColumnType("BLOB");

                    b.HasKey("ImageFileId");

                    b.ToTable("ImageFile");
                });

            modelBuilder.Entity("SQL.ImageInfo", b =>
                {
                    b.Property<int>("ImageInfoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ByteImageImageFileId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClassName")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImageHash")
                        .HasColumnType("TEXT");

                    b.Property<int>("NumOfRequests")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.Property<float>("Prob")
                        .HasColumnType("REAL");

                    b.HasKey("ImageInfoId");

                    b.HasIndex("ByteImageImageFileId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("ImageClassImageInfo", b =>
                {
                    b.HasOne("SQL.ImageClass", null)
                        .WithMany()
                        .HasForeignKey("ImageClassesImageClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SQL.ImageInfo", null)
                        .WithMany()
                        .HasForeignKey("ImagesImageInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SQL.ImageInfo", b =>
                {
                    b.HasOne("SQL.ImageFile", "ByteImage")
                        .WithMany()
                        .HasForeignKey("ByteImageImageFileId");

                    b.Navigation("ByteImage");
                });
#pragma warning restore 612, 618
        }
    }
}
