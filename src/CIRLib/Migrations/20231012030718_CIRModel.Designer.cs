﻿// <auto-generated />
using System;
using CIRLib.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CIRLib.Migrations
{
    [DbContext(typeof(CIRLibContext))]
    [Migration("20231012030718_CIRModel")]
    partial class CIRModel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CategorySourceId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("SourceId");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegistryId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RegistryRefId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("RegistryRefId");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Entry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CIRId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CategoryRefId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChildEntityId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("IdInSource")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Inactive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ParentEntityId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegistryId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RegistryRefId")
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceOwnerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CategoryRefId");

                    b.HasIndex("IdInSource");

                    b.HasIndex("RegistryRefId");

                    b.ToTable("Entry");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Property", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("CategoryId")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DataType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("EntryIdInSource")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EntryRefId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PropertyId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("RegistryId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("EntryRefId");

                    b.HasIndex("PropertyId");

                    b.HasIndex("RegistryId");

                    b.ToTable("Property");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.PropertyValue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PropertyId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PropertyRefId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.HasIndex("PropertyRefId");

                    b.ToTable("PropertyValue");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Registry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegistryId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RegistryId");

                    b.ToTable("Registry");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Category", b =>
                {
                    b.HasOne("CIRLib.ObjectModel.Models.Registry", "Registry")
                        .WithMany("Categories")
                        .HasForeignKey("RegistryRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Registry");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Entry", b =>
                {
                    b.HasOne("CIRLib.ObjectModel.Models.Category", "Category")
                        .WithMany("Entries")
                        .HasForeignKey("CategoryRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CIRLib.ObjectModel.Models.Registry", "Registry")
                        .WithMany("Entries")
                        .HasForeignKey("RegistryRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Registry");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Property", b =>
                {
                    b.HasOne("CIRLib.ObjectModel.Models.Category", null)
                        .WithMany("Properties")
                        .HasForeignKey("CategoryId");

                    b.HasOne("CIRLib.ObjectModel.Models.Entry", "Entry")
                        .WithMany("Property")
                        .HasForeignKey("EntryRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CIRLib.ObjectModel.Models.Registry", null)
                        .WithMany("Properties")
                        .HasForeignKey("RegistryId");

                    b.Navigation("Entry");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.PropertyValue", b =>
                {
                    b.HasOne("CIRLib.ObjectModel.Models.Property", "Property")
                        .WithMany("PropertyValues")
                        .HasForeignKey("PropertyRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Category", b =>
                {
                    b.Navigation("Entries");

                    b.Navigation("Properties");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Entry", b =>
                {
                    b.Navigation("Property");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Property", b =>
                {
                    b.Navigation("PropertyValues");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Registry", b =>
                {
                    b.Navigation("Categories");

                    b.Navigation("Entries");

                    b.Navigation("Properties");
                });
#pragma warning restore 612, 618
        }
    }
}