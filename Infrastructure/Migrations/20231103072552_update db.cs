using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xlm = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fee_service",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fee_type = table.Column<int>(type: "integer", nullable: false),
                    fee_name = table.Column<string>(type: "text", nullable: false),
                    fee_amount_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    fee_amount_currency = table.Column<string>(type: "text", nullable: true),
                    fee_percent = table.Column<float>(type: "real", nullable: true),
                    is_percent = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fee_service", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "image",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    image_link = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_image", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "member_device_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<string>(type: "text", nullable: false),
                    device_token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_device_token", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "member_notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    notification_type = table.Column<string>(type: "text", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_readed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_notification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "member_point_rule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    point_per_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    minimum_amount_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    minimum_amount_currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_point_rule", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "membership_class",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_name = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    percent_default = table.Column<float>(type: "real", nullable: false),
                    percent_birth_date = table.Column<float>(type: "real", nullable: false),
                    max_money_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    max_money_currency = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membership_class", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "news",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    content = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    thumbnail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notification_type = table.Column<string>(type: "text", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "partner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partner", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "phone_validation_check",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    send_code_count = table.Column<int>(type: "integer", nullable: false),
                    last_sent = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_phone_validation_check", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_name = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: false),
                    address_state = table.Column<string>(type: "text", nullable: false),
                    address_zip_code = table.Column<string>(type: "text", nullable: false),
                    address_city = table.Column<string>(type: "text", nullable: false),
                    address_street = table.Column<string>(type: "text", nullable: false),
                    operation = table.Column<int>(type: "integer", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    amenities = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_name = table.Column<string>(type: "character varying(155)", maxLength: 155, nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    price_currency = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id1",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "member_ship_benefit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_ship_benefit", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_ship_benefit_membership_class_membership_class_temp_",
                        column: x => x.membership_class_id,
                        principalTable: "membership_class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    membership_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    member_code = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    identity_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_members_membership_class_membership_class_temp_id",
                        column: x => x.membership_class_id,
                        principalTable: "membership_class",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "voucher",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_voucher = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    point = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    place = table.Column<string>(type: "text", nullable: true),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    qr_code_image_url = table.Column<string>(type: "text", nullable: true),
                    limit_quantity = table.Column<int>(type: "integer", nullable: true),
                    is_voucher_default = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    content_voucher = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_voucher", x => x.id);
                    table.ForeignKey(
                        name: "fk_voucher_partner_partner_temp_id",
                        column: x => x.partner_id,
                        principalTable: "partner",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "promotion-to-restaurant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    restaurant_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_to_restaurant", x => new { x.promotion_id, x.restaurant_id, x.id });
                    table.ForeignKey(
                        name: "fk_promotion_to_restaurant_promotion_promotion_id1",
                        column: x => x.promotion_id,
                        principalTable: "promotion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_promotion_to_restaurant_promotion_promotion_id11",
                        column: x => x.promotion_id1,
                        principalTable: "promotion",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_promotion_to_restaurant_restaurant_restaurant_temp_id",
                        column: x => x.restaurant_id1,
                        principalTable: "restaurants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_promotion_to_restaurant_restaurant_restaurant_temp_id1",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_day",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    begin_shift = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_shift = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_day", x => x.id);
                    table.ForeignKey(
                        name: "fk_work_day_restaurant_restaurant_temp_id2",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_to_role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_to_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_to_role_role_role_temp_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_to_role_user_user_temp_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_code = table.Column<string>(type: "text", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    booking_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_of_people = table.Column<int>(type: "integer", nullable: false),
                    confirmed_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejected_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_member_member_temp_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bookings_restaurant_restaurant_temp_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "member_point_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_point = table.Column<int>(type: "integer", nullable: false),
                    point_type = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_point_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_point_history_member_member_temp_id1",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "member_voucher",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_voucher", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_voucher_member_member_temp_id2",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_member_voucher_voucher_voucher_temp_id",
                        column: x => x.voucher_id,
                        principalTable: "voucher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_code = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_type = table.Column<int>(type: "integer", nullable: false),
                    payment_type = table.Column<int>(type: "integer", nullable: true),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_bill_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_bill_currency = table.Column<string>(type: "text", nullable: false),
                    has_payment = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    receiving_address = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    has_issue_an_invoice = table.Column<bool>(type: "boolean", nullable: false),
                    company_tax_code = table.Column<string>(type: "text", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    company_email = table.Column<string>(type: "text", nullable: false),
                    company_address = table.Column<string>(type: "text", nullable: false),
                    has_request_cutlery = table.Column<bool>(type: "boolean", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery", x => x.id);
                    table.ForeignKey(
                        name: "fk_delivery_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_code = table.Column<string>(type: "text", nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_type = table.Column<int>(type: "integer", nullable: true),
                    order_type = table.Column<int>(type: "integer", nullable: false),
                    total_quantity = table.Column<int>(type: "integer", nullable: false),
                    total_bill_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_bill_currency = table.Column<string>(type: "text", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "line_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    product_image_url = table.Column<string>(type: "text", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    price_currency = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_line_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_line_item_order_order_temp_id1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_line_item_product_product_temp_id1",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_fee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_fee_name = table.Column<string>(type: "text", nullable: false),
                    order_fee_value = table.Column<string>(type: "text", nullable: false),
                    order_fee_charge_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    order_fee_charge_currency = table.Column<string>(type: "text", nullable: false),
                    is_percent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_fee", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_fee_order_order_temp_id2",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviews", x => x.id);
                    table.ForeignKey(
                        name: "fk_reviews_members_member_id1",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reviews_orders_order_id1",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_detail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    price_currency = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_detail_invoice_invoice_temp_id1",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_fee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_fee_name = table.Column<string>(type: "text", nullable: false),
                    invoice_fee_amount = table.Column<string>(type: "text", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fee_change_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    fee_change_currency = table.Column<string>(type: "text", nullable: false),
                    is_percent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_fee", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_fee_invoice_invoice_temp_id2",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_detail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_ref_id = table.Column<string>(type: "text", nullable: false),
                    payment_platform = table.Column<int>(type: "integer", nullable: false),
                    payment_response = table.Column<string>(type: "text", nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_detail_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_member_id",
                table: "bookings",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_restaurant_id",
                table: "bookings",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_order_id",
                table: "delivery",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoice_order_id",
                table: "invoice",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoice_detail_invoice_id",
                table: "invoice_detail",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoice_fee_invoice_id",
                table: "invoice_fee",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_item_order_id",
                table: "line_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_item_product_id",
                table: "line_item",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_device_token_device_token",
                table: "member_device_token",
                column: "device_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_member_device_token_identity_id",
                table: "member_device_token",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_member_point_history_member_id",
                table: "member_point_history",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_ship_benefit_membership_class_id",
                table: "member_ship_benefit",
                column: "membership_class_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_voucher_member_id",
                table: "member_voucher",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_voucher_voucher_id",
                table: "member_voucher",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "ix_members_email",
                table: "members",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_members_identity_id",
                table: "members",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_members_membership_class_id",
                table: "members",
                column: "membership_class_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_fee_order_id",
                table: "order_fee",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_booking_id",
                table: "orders",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_member_id",
                table: "orders",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_detail_invoice_id",
                table: "payment_detail",
                column: "invoice_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_to_restaurant_promotion_id1",
                table: "promotion-to-restaurant",
                column: "promotion_id1");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_to_restaurant_restaurant_id",
                table: "promotion-to-restaurant",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_to_restaurant_restaurant_id1",
                table: "promotion-to-restaurant",
                column: "restaurant_id1");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_member_id",
                table: "reviews",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_order_id",
                table: "reviews",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_to_role_role_id",
                table: "user_to_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_to_role_user_id",
                table: "user_to_role",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_voucher_partner_id",
                table: "voucher",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "ix_work_day_restaurant_id",
                table: "work_day",
                column: "restaurant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "delivery");

            migrationBuilder.DropTable(
                name: "fee_service");

            migrationBuilder.DropTable(
                name: "image");

            migrationBuilder.DropTable(
                name: "invoice_detail");

            migrationBuilder.DropTable(
                name: "invoice_fee");

            migrationBuilder.DropTable(
                name: "line_item");

            migrationBuilder.DropTable(
                name: "member_device_token");

            migrationBuilder.DropTable(
                name: "member_notification");

            migrationBuilder.DropTable(
                name: "member_point_history");

            migrationBuilder.DropTable(
                name: "member_point_rule");

            migrationBuilder.DropTable(
                name: "member_ship_benefit");

            migrationBuilder.DropTable(
                name: "member_voucher");

            migrationBuilder.DropTable(
                name: "news");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "order_fee");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "payment_detail");

            migrationBuilder.DropTable(
                name: "phone_validation_check");

            migrationBuilder.DropTable(
                name: "promotion-to-restaurant");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "user_to_role");

            migrationBuilder.DropTable(
                name: "work_day");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "voucher");

            migrationBuilder.DropTable(
                name: "invoice");

            migrationBuilder.DropTable(
                name: "promotion");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "partner");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "restaurants");

            migrationBuilder.DropTable(
                name: "membership_class");
        }
    }
}
