# 🎯 Server-side DataTable + Edit/Delete modal for Items

## Understanding
Add server-side DataTable support for the items list and make Edit/Delete work via modal dialogs. Implement server endpoints for listing, fetching a single item and deleting, and update the Add endpoint to persist create/update (including image upload). Wire up client-side JS to initialize DataTable, load data from server, and handle edit/delete modal behavior.

## Front-end pages (public site) — (show first)
These are the public-facing URLs and short descriptions to use in navigation and documentation:

- Home
  - URL: https://localhost:7229/
  - Description: Public landing page showcasing featured menu items, promotions and primary navigation.

- About Us
  - URL: https://localhost:7229/Home/About
  - Description: Company story, mission and opening hours.

- Blog
  - URL: https://localhost:7229/Home/Blog
  - Description: Blog listing of posts, news and articles.

- Contact / Contact Us
  - URL: https://localhost:7229/Home/Contact
  - Description: Contact form, phone number and address.

## Admin login / forgot-password notes

- Admin login URL: https://localhost:7229/admin
- Seeded admin credentials (development only):
  - Email: admin@admin.com
  - Password: Admin@123
- After successful login the admin can access dashboard, blog, category and item pages protected with [Authorize].
- Logout endpoint: /admin/login/logout (link placed in admin header).
- Forgot-password flow: /admin/forgot-password sends a tokenized reset link via SMTP (Gmail settings required in appsettings). The reset link (/admin/reset-password?token=...) lets the admin set a new password.

Note: The seeded credentials are for development only. Remove or change them before deploying to production.

## Module-wise examples
Below are concise module templates you can apply project-wide. Each module follows the same pattern: Admin view, server-side DataTable list endpoint, single-get, add/update, delete, and client JS to operate modals.

- Category (example)
  - Heading: Category Management
  - Description: Manage product categories (name, image, status). Server-side DataTable supports paging, searching and ordering.
  - Fields: Id, CategoryName, CategoryImage, Status, CreatedAt, UpdatedAt
  - Endpoints:
	- GET /admin/category -> returns view with DataTable markup
	- POST /admin/category/list -> DataTables server-side JSON (draw, recordsTotal, recordsFiltered, data)
	- GET /admin/category/get/{id} -> returns category JSON for edit
	- POST /admin/category/add -> create or update Category (IFormFile upload for CategoryImage)
	- POST /admin/category/delete -> delete category by id (also delete file)
  - View: Views/Admin/Category/Index.cshtml with Add/Edit modal, Delete confirmation modal, include _ValidationScriptsPartial
  - Client JS: getCategories() initializes server-side DataTable, render action buttons, .btn-edit fetches /get/{id}, populate modal, .btn-delete opens confirm modal and calls /delete.
  - Notes: store images under wwwroot/uploads/category; show preview via /uploads/category/{fileName}

- Item
  - Heading: Item Management
  - Description: CRUD for menu items. Items are linked to Category and include price, description and image.
  - Fields: Id, ItemName, ItemDetail, Price(decimal), CategoryId (FK), ItemImage, Status, CreatedAt, UpdatedAt
  - Endpoints:
	- GET /admin/item
	- POST /admin/item/list
	- GET /admin/item/get/{id}
	- POST /admin/item/add
	- POST /admin/item/delete
  - View/JS: same modal + DataTable pattern as Category. Use select for Category (bind from /admin/category/list or a small endpoint returning categories).
  - Notes: store images under wwwroot/uploads/items; price precision configured on model; use EF include to read Category name in list.

- Blog
  - Heading: Blog Management
  - Description: Manage blog posts, category tag, image and content. Server-side DataTable enables fast browsing.
  - Fields: Id, Title, CategoryId, Image, Description (html), Status, CreatedAt, UpdatedAt
  - Endpoints: /admin/blog (view), POST /admin/blog/list, GET /admin/blog/get/{id}, POST /admin/blog/add, POST /admin/blog/delete
  - View/JS: DataTable + Add/Edit modal (rich text editor for Description), image upload via FileUploadHelper (/uploads/blog).

- AdminUser (profile & password)
  - Heading: Admin Users
  - Description: Manage admin account profile, change password and forgot/reset password.
  - Fields: Id, Email, PasswordHash, FullName, ProfileImage, PasswordResetToken, PasswordResetExpires, Status, CreatedAt, UpdatedAt
  - Endpoints (already implemented): /admin/login (login/logout), /admin/profile (view/update), /admin/forgot-password, /admin/reset-password
  - Notes: Passwords hashed via PasswordHasher<T>; forgot-password sends tokenized reset link via SMTP; add migration to persist new columns.

## Implementation checklist (per module)
1. Model: add fields to entity and DbSet if required.
2. DbContext: update DbSet and run EF migration.
3. Controller: implement server-side DataTable list, get, add, delete endpoints.
4. View: create Index.cshtml with DataTable markup, Add/Edit modal, Delete modal and _ValidationScriptsPartial.
5. Client JS: implement get<Module>() to init DataTable and handlers for add/edit/delete using AJAX/FormData.
6. File uploads: use FileUploadHelper.UploadFile and DeleteFile; store under wwwroot/uploads/<module>.
7. Test: verify list, search, ordering, add/update (including image), delete and ModelState mapping for validation messages.

## Risks & Open Questions
- File upload storage path and URL prefix used in CategoryController (/uploads/item) — will follow same pattern.
- DataTables JS/CSS must be included in layout; if not, table won't initialize.
- The UI markup for showing existing image in edit modal will be minimal (filename preview only).

**Last Updated**: 2026-08-02 16:21:18

## 📝 Plan Steps
-  **Update Controllers/Admin/ItemController.cs to add DB + env, implement List, Get, Delete and update Add to persist and upload image.**
-  **Add removeItemModal markup to Views/Admin/Item/Index.cshtml.**
-  **Implement getItems() JS to initialize server-side DataTable and render action buttons.**
-  **Add JS handlers for Edit button: fetch item, populate form fields (including selecting category) and open modal.**
-  **Add JS handlers for Delete: open confirmation modal and call delete endpoint, refresh DataTable on success.**
-  **Test locally and adjust field name normalization if ModelState keys include prefixes.**


