﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace respNewsV8.Models;

public partial class RespNewContext : DbContext
{
    public RespNewContext()
    {
    }

    public RespNewContext(DbContextOptions<RespNewContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Infographic> Infographics { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Messagess> Messagesses { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<NewsPhoto> NewsPhotos { get; set; }

    public virtual DbSet<NewsTag> NewsTags { get; set; }

    public virtual DbSet<NewsVideo> NewsVideos { get; set; }

    public virtual DbSet<Newspaper> Newspapers { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Statisticss> Statisticsses { get; set; }

    public virtual DbSet<Subscriber> Subscribers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-353APLF\\SQLEXPRESS;Database=respNew;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0BB603DD2C");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryName).HasMaxLength(50);

            entity.HasOne(d => d.CategoryLang).WithMany(p => p.Categories)
                .HasForeignKey(d => d.CategoryLangId)
                .HasConstraintName("FK__Category__Catego__40058253");
        });

        modelBuilder.Entity<Infographic>(entity =>
        {
            entity.HasKey(e => e.InfId).HasName("PK__Infograp__99CF2C7303FBDB42");

            entity.Property(e => e.InfName).HasMaxLength(200);
            entity.Property(e => e.InfPostDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__B93855AB80146FC9");

            entity.Property(e => e.LanguageName).HasMaxLength(20);
        });

        modelBuilder.Entity<Messagess>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C671DF744");

            entity.ToTable("Messagess");

            entity.Property(e => e.MessageDate).HasColumnType("datetime");
            entity.Property(e => e.MessageIsRead).HasDefaultValue(false);
            entity.Property(e => e.MessageMail).HasMaxLength(30);
            entity.Property(e => e.MessageTitle).HasMaxLength(100);
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.NewsId).HasName("PK__News__954EBDF3AFC14B36");

            entity.Property(e => e.NewsDate).HasColumnType("datetime");
            entity.Property(e => e.NewsTitle)
                .HasMaxLength(3000)
                .IsUnicode(false);
            entity.Property(e => e.NewsUpdateDate).HasColumnType("datetime");
            entity.Property(e => e.NewsViewCount).HasDefaultValue(1);

            entity.HasOne(d => d.NewsAdmin).WithMany(p => p.News)
                .HasForeignKey(d => d.NewsAdminId)
                .HasConstraintName("FK__News__NewsAdminI__540C7B00");

            entity.HasOne(d => d.NewsCategory).WithMany(p => p.News)
                .HasForeignKey(d => d.NewsCategoryId)
                .HasConstraintName("FK__News__NewsCatego__3E52440B");

            entity.HasOne(d => d.NewsLang).WithMany(p => p.News)
                .HasForeignKey(d => d.NewsLangId)
                .HasConstraintName("FK__News__NewsLangId__534D60F1");

            entity.HasOne(d => d.NewsOwner).WithMany(p => p.News)
                .HasForeignKey(d => d.NewsOwnerId)
                .HasConstraintName("FK__News__NewsOwnerI__1F98B2C1");
        });

        modelBuilder.Entity<NewsPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__NewsPhot__21B7B5E2DC28F821");

            entity.Property(e => e.PhotoUrl).HasColumnName("PhotoURL");

            entity.HasOne(d => d.PhotoNews).WithMany(p => p.NewsPhotos)
                .HasForeignKey(d => d.PhotoNewsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__NewsPhoto__Photo__5AEE82B9");
        });

        modelBuilder.Entity<NewsTag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__NewsTags__657CF9AC6C2A974F");

            entity.Property(e => e.TagName).HasMaxLength(100);

            entity.HasOne(d => d.TagNews).WithMany(p => p.NewsTags)
                .HasForeignKey(d => d.TagNewsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__NewsTags__TagNew__3F115E1A");
        });

        modelBuilder.Entity<NewsVideo>(entity =>
        {
            entity.HasKey(e => e.VideoId).HasName("PK__NewsVide__BAE5126A9A0229F7");

            entity.Property(e => e.VideoUrl).HasColumnName("VideoURL");

            entity.HasOne(d => d.VideoNews).WithMany(p => p.NewsVideos)
                .HasForeignKey(d => d.VideoNewsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__NewsVideo__Video__0D7A0286");
        });

        modelBuilder.Entity<Newspaper>(entity =>
        {
            entity.HasKey(e => e.NewspaperId).HasName("PK__Newspape__84EBB4804D0326A1");

            entity.Property(e => e.NewspaperDate).HasColumnType("datetime");
            entity.Property(e => e.NewspaperPrice)
                .HasMaxLength(20)
                .HasDefaultValue("Xeyr");
            entity.Property(e => e.NewspaperStatus).HasDefaultValue(true);
            entity.Property(e => e.NewspaperTitle).HasMaxLength(200);
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__Owners__819385B80E4F4638");

            entity.Property(e => e.OwnerName).HasMaxLength(50);
            entity.Property(e => e.OwnerTotal).HasDefaultValue(0);
        });

        modelBuilder.Entity<Statisticss>(entity =>
        {
            entity.HasKey(e => e.StatisticId).HasName("PK__Statisti__367DEB175CCD9B95");

            entity.ToTable("Statisticss");

            entity.HasIndex(e => new { e.VisitorIp, e.VisitDate }, "UQ__Statisti__24F31FC7C376A412").IsUnique();

            entity.Property(e => e.IsAzLanguage).HasDefaultValue(false);
            entity.Property(e => e.IsDesktop).HasDefaultValue(false);
            entity.Property(e => e.IsEngLanguage).HasDefaultValue(false);
            entity.Property(e => e.IsMobile).HasDefaultValue(false);
            entity.Property(e => e.IsRuLanguage).HasDefaultValue(false);
            entity.Property(e => e.VisitCount).HasDefaultValue(1);
            entity.Property(e => e.VisitDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VisitorCity)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.VisitorCountry)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.VisitorIp)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("VisitorIP");
        });

        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.HasKey(e => e.SubId).HasName("PK__Subscrib__4D9BB84AC2FACF06");

            entity.Property(e => e.SubDate).HasColumnType("datetime");
            entity.Property(e => e.SubEmail).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CCF65F3AF");

            entity.Property(e => e.UserName).HasMaxLength(30);
            entity.Property(e => e.UserPassword).HasMaxLength(30);
            entity.Property(e => e.UserRole).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}