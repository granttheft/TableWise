namespace Tablewise.Infrastructure.Persistence.SeedData;

/// <summary>
/// Seed data için kullanılan sabit ID'ler. Test ve development ortamında tutarlılık sağlar.
/// </summary>
internal static class SeedIds
{
    // Plans
    public static readonly Guid PlanStarterId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PlanProId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid PlanBusinessId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PlanEnterpriseId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    // Demo Tenant & SuperAdmin
    public static readonly Guid SuperAdminUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid DemoTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid DemoVenueId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid DemoSubscriptionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid DemoOwnerUserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    // Tables
    public static readonly Guid Table1Id = Guid.Parse("e0000001-0000-0000-0000-000000000001");
    public static readonly Guid Table2Id = Guid.Parse("e0000002-0000-0000-0000-000000000002");
    public static readonly Guid Table3Id = Guid.Parse("e0000003-0000-0000-0000-000000000003");
    public static readonly Guid Table4Id = Guid.Parse("e0000004-0000-0000-0000-000000000004");
    public static readonly Guid Table5Id = Guid.Parse("e0000005-0000-0000-0000-000000000005");

    // Table Combination
    public static readonly Guid TableCombo1Id = Guid.Parse("e0000010-0000-0000-0000-000000000010");

    // Venue Closures
    public static readonly Guid VenueClosure1Id = Guid.Parse("e0000020-0000-0000-0000-000000000020");
    public static readonly Guid VenueClosure2Id = Guid.Parse("e0000021-0000-0000-0000-000000000021");

    // Custom Fields
    public static readonly Guid CustomField1Id = Guid.Parse("e0000030-0000-0000-0000-000000000030");
    public static readonly Guid CustomField2Id = Guid.Parse("e0000031-0000-0000-0000-000000000031");
    public static readonly Guid CustomField3Id = Guid.Parse("e0000032-0000-0000-0000-000000000032");

    // Rules
    public static readonly Guid Rule1Id = Guid.Parse("e0000040-0000-0000-0000-000000000040");
    public static readonly Guid Rule2Id = Guid.Parse("e0000041-0000-0000-0000-000000000041");
    public static readonly Guid Rule3Id = Guid.Parse("e0000042-0000-0000-0000-000000000042");
    public static readonly Guid Rule4Id = Guid.Parse("e0000043-0000-0000-0000-000000000043");
    public static readonly Guid Rule5Id = Guid.Parse("e0000044-0000-0000-0000-000000000044");

    // Customers (15 adet)
    public static readonly Guid Customer1Id = Guid.Parse("e0000050-0000-0000-0000-000000000050");
    public static readonly Guid Customer2Id = Guid.Parse("e0000051-0000-0000-0000-000000000051");
    public static readonly Guid Customer3Id = Guid.Parse("e0000052-0000-0000-0000-000000000052");
    public static readonly Guid Customer4Id = Guid.Parse("e0000053-0000-0000-0000-000000000053");
    public static readonly Guid Customer5Id = Guid.Parse("e0000054-0000-0000-0000-000000000054");
    public static readonly Guid Customer6Id = Guid.Parse("e0000055-0000-0000-0000-000000000055");
    public static readonly Guid Customer7Id = Guid.Parse("e0000056-0000-0000-0000-000000000056");
    public static readonly Guid Customer8Id = Guid.Parse("e0000057-0000-0000-0000-000000000057");
    public static readonly Guid Customer9Id = Guid.Parse("e0000058-0000-0000-0000-000000000058");
    public static readonly Guid Customer10Id = Guid.Parse("e0000059-0000-0000-0000-000000000059");
    public static readonly Guid Customer11Id = Guid.Parse("e000005a-0000-0000-0000-00000000005a");
    public static readonly Guid Customer12Id = Guid.Parse("e000005b-0000-0000-0000-00000000005b");
    public static readonly Guid Customer13Id = Guid.Parse("e000005c-0000-0000-0000-00000000005c");
    public static readonly Guid Customer14Id = Guid.Parse("e000005d-0000-0000-0000-00000000005d");
    public static readonly Guid Customer15Id = Guid.Parse("e000005e-0000-0000-0000-00000000005e");
}
