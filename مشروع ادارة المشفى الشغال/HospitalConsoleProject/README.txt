HospitalConsoleProject - النسخة المعدلة

أهم التعديلات:
1- تم استخدام Linked List حقيقية للأطباء والمرضى عن طريق Node و SortedLinkedList بدل List.
2- كل طبيب صار له DepartmentName مثل: عينية، جلدية، إسعاف.
3- المعالجة صارت تحتوي DepartmentName و DoctorIds حتى يظهر الطبيب والقسم مع كل معالجة.
4- المريض الداخلي صار لديه سجل معالجات داخلية وسجل معالجات خارجية منفصلين.
5- الطبيب المتعاقد يحسب 50% من كل المعالجات المرتبطة به.
6- تمت إضافة فلترة المعالجات ضمن فترة زمنية وتعرض: المريض، الطبيب، القسم، الكلفة، النوع.
7- تمت إضافة حفظ وقراءة من ملفات نصية: doctors.txt, patients.txt, treatments.txt.

ملاحظة:
المشروع TargetFramework = net8.0. افتحي ملف HospitalConsoleProject.csproj أو Solution إن أنشأه Visual Studio.
