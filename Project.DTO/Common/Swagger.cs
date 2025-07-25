namespace Project.DTO.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExclude : Attribute { }


    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExcludeRequest : Attribute { }


    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExcludeResponse : Attribute { }



}
