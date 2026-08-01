export function up(knex) {
  return knex.schema.alterTable('users', (table) => {
    table.string('contact_link').nullable();
    table.string('cv_url').nullable();
  });
}

export function down(knex) {
  return knex.schema.alterTable('users', (table) => {
    table.dropColumn('cv_url');
    table.dropColumn('contact_link');
  });
}
