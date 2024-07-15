USE [BDInventarios]
GO
INSERT [dbo].[PERMISOS] ([IdPermisos], [Descripcion], [Salidas], [Entradas], [Productos], [Clientes], [Proveedores], [Inventario], [Configuracion]) VALUES (1, N'Administrador', 1, 1, 1, 1, 1, 1, 1)
INSERT [dbo].[PERMISOS] ([IdPermisos], [Descripcion], [Salidas], [Entradas], [Productos], [Clientes], [Proveedores], [Inventario], [Configuracion]) VALUES (2, N'Empleado', 1, 1, 1, 1, 1, 1, 0)
GO
INSERT [dbo].[USUARIO] ([IdUsuario], [NombreCompleto], [NombreUsuario], [Clave], [IdPermisos]) VALUES (1, N'Root', N'Admin', N'123', 1)
INSERT [dbo].[USUARIO] ([IdUsuario], [NombreCompleto], [NombreUsuario], [Clave], [IdPermisos]) VALUES (2, N'Jhon Bohorquez', N'fhgk', N'123', 2)
GO
