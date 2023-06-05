﻿// <auto-generated />
using System;
using CIRLib.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CIRLib.Migrations
{
    [DbContext(typeof(CIRLibContext))]
    partial class CIRLibContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Category", b =>
                {
                    b.Property<string>("CategoryId")
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryDescription")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Description");

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

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegistryRefId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("CategoryId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("RegistryRefId");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Entry", b =>
                {
                    b.Property<string>("IdInSource")
                        .HasColumnType("TEXT");

                    b.Property<string>("CIRId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryRefId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("EntryDescription")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Description");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Inactive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegistryRefId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceOwnerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("IdInSource");

                    b.HasIndex("CategoryRefId");

                    b.HasIndex("IdInSource");

                    b.HasIndex("RegistryRefId");

                    b.ToTable("Entry");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Property", b =>
                {
                    b.Property<string>("PropertyId")
                        .HasColumnType("TEXT");

                    b.Property<string>("CategoryRefId")
                        .IsRequired()
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

                    b.Property<string>("EntryRefIdInSource")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PropertyValue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RegistryRefId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("PropertyId");

                    b.HasIndex("CategoryRefId");

                    b.HasIndex("EntryRefIdInSource");

                    b.HasIndex("PropertyId");

                    b.HasIndex("RegistryRefId");

                    b.ToTable("Property");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.PropertyValue", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PropertyRefId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("Key");

                    b.HasIndex("PropertyRefId");

                    b.ToTable("PropertyValue");
                });

            modelBuilder.Entity("CIRLib.ObjectModel.Models.Registry", b =>
                {
                    b.Property<string>("RegistryId")
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

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("RegistryId");

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
                    b.HasOne("CIRLib.ObjectModel.Models.Category", "Category")
                        .WithMany("Properties")
                        .HasForeignKey("CategoryRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CIRLib.ObjectModel.Models.Entry", "Entry")
                        .WithMany("Property")
                        .HasForeignKey("EntryRefIdInSource")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CIRLib.ObjectModel.Models.Registry", "Registry")
                        .WithMany("Properties")
                        .HasForeignKey("RegistryRefId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Entry");

                    b.Navigation("Registry");
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
