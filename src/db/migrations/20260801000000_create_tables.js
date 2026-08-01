export function up(knex) {
  return knex.schema
    .createTable('users', (table) => {
      table.uuid('id').primary();
      table.string('email').unique().notNullable();
      table.string('name').notNullable();
      table.enum('role', ['Student', 'Founder', 'Manager', 'Guest']).notNullable().defaultTo('Guest');
      table.string('student_id').nullable();
      table.timestamps(true, true);
    })
    .createTable('projects', (table) => {
      table.uuid('id').primary();
      table.string('name').unique().notNullable();
      table.string('pitch').notNullable();
      table.text('description').nullable();
      table.enum('status', ['Active', 'At-Risk', 'Suspended']).notNullable().defaultTo('Active');
      table.timestamp('last_updated_at').notNullable().defaultTo(knex.fn.now());
      table.bigInteger('storage_used_bytes').notNullable().defaultTo(0);
      table.timestamps(true, true);

      // Performance Index for daily dormancy checks (BR-08)
      table.index(['last_updated_at', 'status']);
    })
    .createTable('milestones', (table) => {
      table.uuid('id').primary();
      table.uuid('project_id').references('id').inTable('projects').onDelete('CASCADE').notNullable();
      table.string('title').notNullable();
      table.text('description').nullable();
      table.boolean('done').notNullable().defaultTo(false);
      table.timestamp('date_completed').nullable();
      table.timestamp('created_at').notNullable().defaultTo(knex.fn.now());
    })
    .createTable('jobs', (table) => {
      table.uuid('id').primary();
      table.uuid('project_id').references('id').inTable('projects').onDelete('CASCADE').notNullable();
      table.string('title').notNullable();
      table.enum('category', ['Engineering', 'Business', 'Design', 'Marketing']).notNullable();
      table.text('description').notNullable();
      table.text('requirements').nullable();
      table.enum('status', ['Open', 'Closed']).notNullable().defaultTo('Open');
      table.timestamp('created_at').notNullable().defaultTo(knex.fn.now());
    })
    .createTable('applications', (table) => {
      table.uuid('id').primary();
      table.uuid('student_id').references('id').inTable('users').onDelete('CASCADE').notNullable();
      table.uuid('job_id').references('id').inTable('jobs').onDelete('CASCADE').notNullable();
      table.enum('status', ['Pending', 'Approved', 'Rejected']).notNullable().defaultTo('Pending');
      table.timestamp('created_at').notNullable().defaultTo(knex.fn.now());

      // Performance Index for concurrent application checking (BR-05)
      table.index(['student_id', 'status']);
    });
}

export function down(knex) {
  return knex.schema
    .dropTableIfExists('applications')
    .dropTableIfExists('jobs')
    .dropTableIfExists('milestones')
    .dropTableIfExists('projects')
    .dropTableIfExists('users');
}
