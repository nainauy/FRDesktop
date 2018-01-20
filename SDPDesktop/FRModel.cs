namespace SDPDesktop
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FRModel : DbContext
    {
        public FRModel()
            : base("name=FRModel")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<EntranceRegister> EntranceRegisters { get; set; }
        public virtual DbSet<Gender> Genders { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Sector> Sectors { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Visitor> Visitors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRole>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.AspNetUserClaims)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.AspNetUserLogins)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Gender>()
                .HasMany(e => e.Users)
                .WithOptional(e => e.Gender)
                .HasForeignKey(e => e.Gender_Id);

            modelBuilder.Entity<Gender>()
                .HasMany(e => e.Visitors)
                .WithOptional(e => e.Gender)
                .HasForeignKey(e => e.Gender_Id);

            modelBuilder.Entity<Sector>()
                .HasMany(e => e.Users)
                .WithOptional(e => e.Sector)
                .HasForeignKey(e => e.Sector_Id);
        }
    }
}
