namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;
    using System.Xml;
    using System.Xml.Schema;

    public class TrackingProfileSerializer
    {
        private const string _ns = "http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile";
        private XmlSchema _schema = XmlSchema.Read(new StringReader("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<xs:schema id=\"WFTrackingProfile\" targetNamespace=\"http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile\" elementFormDefault=\"qualified\" xmlns=\"http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n    <xs:element name=\"TrackingProfile\" type=\"TrackingProfileType\" />\r\n\r\n    <xs:complexType name=\"TrackingProfileType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"TrackPoints\" type=\"TrackPointListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n        <xs:attribute name=\"version\" type=\"VersionType\" />\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"TrackPointListType\">\r\n        <xs:sequence>\r\n            <xs:choice minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n                <xs:element name=\"ActivityTrackPoint\" type=\"ActivityTrackPointType\" />\r\n                <xs:element name=\"UserTrackPoint\" type=\"UserTrackPointType\" />\r\n                <xs:element name=\"WorkflowTrackPoint\" type=\"WorkflowTrackPointType\"  />\r\n            </xs:choice>\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityTrackPointType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"MatchingLocations\" minOccurs=\"1\" maxOccurs=\"1\" type=\"IncludeActivityTrackingLocationListType\" />\r\n            <xs:element name=\"ExcludedLocations\" minOccurs=\"0\" maxOccurs=\"1\" type=\"ExcludeActivityTrackingLocationListType\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Extracts\" type=\"ExtractListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"IncludeActivityTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ActivityTrackingLocation\" type=\"ActivityTrackingLocationType\" minOccurs=\"1\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ExcludeActivityTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ActivityTrackingLocation\" type=\"ActivityTrackingLocationType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityTrackingLocationType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"Activity\" type=\"Type\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"ExecutionStatusEvents\" type=\"ExecutionStatusEventListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Conditions\" type=\"ConditionListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"UserTrackPointType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"MatchingLocations\" type=\"IncludeUserTrackingLocationListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"ExcludedLocations\" type=\"ExcludeUserTrackingLocationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Extracts\" type=\"ExtractListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"IncludeUserTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"UserTrackingLocation\" type=\"UserTrackingLocationType\" minOccurs=\"1\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ExcludeUserTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"UserTrackingLocation\" type=\"UserTrackingLocationType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"UserTrackingLocationType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"Activity\" type=\"Type\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"KeyName\" type=\"NonNullString\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Argument\" type=\"Type\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Conditions\" type=\"ConditionListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowTrackPointType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"MatchingLocation\" type=\"WorkflowTrackingLocationMatchType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowTrackingLocationMatchType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"WorkflowTrackingLocation\" type=\"WorkflowTrackingLocationType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowTrackingLocationType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"TrackingWorkflowEvents\" type=\"TrackingWorkflowEventListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"Type\">\r\n        <xs:sequence>\r\n            <xs:choice>\r\n                <xs:element name=\"TypeName\" type=\"NonNullString\" />\r\n                <xs:element name=\"Type\" type=\"NonNullString\" />\r\n            </xs:choice>\r\n            <xs:element name=\"MatchDerivedTypes\" type=\"xs:boolean\" minOccurs=\"1\" maxOccurs=\"1\" default=\"false\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"AnnotationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"Annotation\" type=\"xs:string\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ConditionListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ActivityTrackingCondition\" type=\"ActivityTrackingConditionType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityTrackingConditionType\">\r\n        <xs:sequence minOccurs=\"1\" maxOccurs=\"1\">\r\n            <xs:element name=\"Operator\" type=\"OperatorType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Member\" type=\"NonNullString\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Value\" type=\"xs:string\" minOccurs=\"0\" maxOccurs=\"1\" nillable=\"true\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:simpleType name=\"OperatorType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:enumeration value=\"Equals\" />\r\n            <xs:enumeration value=\"NotEquals\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:complexType name=\"ExecutionStatusEventListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ExecutionStatus\" type=\"ExecutionStatusType\" minOccurs=\"1\" maxOccurs=\"6\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:simpleType name=\"ExecutionStatusType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:enumeration value=\"Initialized\" />\r\n            <xs:enumeration value=\"Executing\" />\r\n            <xs:enumeration value=\"Compensating\" />\r\n            <xs:enumeration value=\"Canceling\" />\r\n            <xs:enumeration value=\"Closed\" />\r\n            <xs:enumeration value=\"Faulting\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:complexType name=\"TrackingWorkflowEventListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"TrackingWorkflowEvent\" type=\"TrackingWorkflowEventType\" minOccurs=\"1\" maxOccurs=\"13\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n        \r\n    <xs:simpleType name=\"TrackingWorkflowEventType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:enumeration value=\"Created\" />\r\n            <xs:enumeration value=\"Completed\" />\r\n            <xs:enumeration value=\"Idle\" />\r\n            <xs:enumeration value=\"Suspended\" />\r\n            <xs:enumeration value=\"Resumed\" />\r\n            <xs:enumeration value=\"Persisted\" />\r\n            <xs:enumeration value=\"Unloaded\" />\r\n            <xs:enumeration value=\"Loaded\" />\r\n            <xs:enumeration value=\"Exception\" />\r\n            <xs:enumeration value=\"Terminated\" />\r\n            <xs:enumeration value=\"Aborted\" />\r\n            <xs:enumeration value=\"Changed\" />\r\n            <xs:enumeration value=\"Started\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:complexType name=\"ExtractListType\">\r\n        <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n            <xs:element name=\"ActivityDataTrackingExtract\" type=\"ActivityDataTrackingExtractType\" />\r\n            <xs:element name=\"WorkflowDataTrackingExtract\" type=\"WorkflowDataTrackingExtractType\" />\r\n        </xs:choice>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityDataTrackingExtractType\">\r\n        <xs:sequence minOccurs=\"1\" maxOccurs=\"1\">\r\n            <xs:element name=\"Member\" type=\"xs:string\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowDataTrackingExtractType\">\r\n        <xs:sequence minOccurs=\"1\" maxOccurs=\"1\">\r\n            <xs:element name=\"Member\" type=\"xs:string\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:simpleType name=\"VersionType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:pattern value=\"(^(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)|(^(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)|(^(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:simpleType name=\"NonNullString\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:minLength value=\"1\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n</xs:schema>"), null);
        private List<ValidationEventArgs> _vArgs = new List<ValidationEventArgs>();
        private bool _vex;
        internal const string _xsd = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<xs:schema id=\"WFTrackingProfile\" targetNamespace=\"http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile\" elementFormDefault=\"qualified\" xmlns=\"http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n    <xs:element name=\"TrackingProfile\" type=\"TrackingProfileType\" />\r\n\r\n    <xs:complexType name=\"TrackingProfileType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"TrackPoints\" type=\"TrackPointListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n        <xs:attribute name=\"version\" type=\"VersionType\" />\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"TrackPointListType\">\r\n        <xs:sequence>\r\n            <xs:choice minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n                <xs:element name=\"ActivityTrackPoint\" type=\"ActivityTrackPointType\" />\r\n                <xs:element name=\"UserTrackPoint\" type=\"UserTrackPointType\" />\r\n                <xs:element name=\"WorkflowTrackPoint\" type=\"WorkflowTrackPointType\"  />\r\n            </xs:choice>\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityTrackPointType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"MatchingLocations\" minOccurs=\"1\" maxOccurs=\"1\" type=\"IncludeActivityTrackingLocationListType\" />\r\n            <xs:element name=\"ExcludedLocations\" minOccurs=\"0\" maxOccurs=\"1\" type=\"ExcludeActivityTrackingLocationListType\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Extracts\" type=\"ExtractListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"IncludeActivityTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ActivityTrackingLocation\" type=\"ActivityTrackingLocationType\" minOccurs=\"1\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ExcludeActivityTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ActivityTrackingLocation\" type=\"ActivityTrackingLocationType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityTrackingLocationType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"Activity\" type=\"Type\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"ExecutionStatusEvents\" type=\"ExecutionStatusEventListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Conditions\" type=\"ConditionListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"UserTrackPointType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"MatchingLocations\" type=\"IncludeUserTrackingLocationListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"ExcludedLocations\" type=\"ExcludeUserTrackingLocationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Extracts\" type=\"ExtractListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"IncludeUserTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"UserTrackingLocation\" type=\"UserTrackingLocationType\" minOccurs=\"1\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ExcludeUserTrackingLocationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"UserTrackingLocation\" type=\"UserTrackingLocationType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"UserTrackingLocationType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"Activity\" type=\"Type\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"KeyName\" type=\"NonNullString\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Argument\" type=\"Type\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Conditions\" type=\"ConditionListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowTrackPointType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"MatchingLocation\" type=\"WorkflowTrackingLocationMatchType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowTrackingLocationMatchType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"WorkflowTrackingLocation\" type=\"WorkflowTrackingLocationType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowTrackingLocationType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"TrackingWorkflowEvents\" type=\"TrackingWorkflowEventListType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"Type\">\r\n        <xs:sequence>\r\n            <xs:choice>\r\n                <xs:element name=\"TypeName\" type=\"NonNullString\" />\r\n                <xs:element name=\"Type\" type=\"NonNullString\" />\r\n            </xs:choice>\r\n            <xs:element name=\"MatchDerivedTypes\" type=\"xs:boolean\" minOccurs=\"1\" maxOccurs=\"1\" default=\"false\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"AnnotationListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"Annotation\" type=\"xs:string\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ConditionListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ActivityTrackingCondition\" type=\"ActivityTrackingConditionType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityTrackingConditionType\">\r\n        <xs:sequence minOccurs=\"1\" maxOccurs=\"1\">\r\n            <xs:element name=\"Operator\" type=\"OperatorType\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Member\" type=\"NonNullString\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Value\" type=\"xs:string\" minOccurs=\"0\" maxOccurs=\"1\" nillable=\"true\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:simpleType name=\"OperatorType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:enumeration value=\"Equals\" />\r\n            <xs:enumeration value=\"NotEquals\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:complexType name=\"ExecutionStatusEventListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"ExecutionStatus\" type=\"ExecutionStatusType\" minOccurs=\"1\" maxOccurs=\"6\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:simpleType name=\"ExecutionStatusType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:enumeration value=\"Initialized\" />\r\n            <xs:enumeration value=\"Executing\" />\r\n            <xs:enumeration value=\"Compensating\" />\r\n            <xs:enumeration value=\"Canceling\" />\r\n            <xs:enumeration value=\"Closed\" />\r\n            <xs:enumeration value=\"Faulting\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:complexType name=\"TrackingWorkflowEventListType\">\r\n        <xs:sequence>\r\n            <xs:element name=\"TrackingWorkflowEvent\" type=\"TrackingWorkflowEventType\" minOccurs=\"1\" maxOccurs=\"13\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n        \r\n    <xs:simpleType name=\"TrackingWorkflowEventType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:enumeration value=\"Created\" />\r\n            <xs:enumeration value=\"Completed\" />\r\n            <xs:enumeration value=\"Idle\" />\r\n            <xs:enumeration value=\"Suspended\" />\r\n            <xs:enumeration value=\"Resumed\" />\r\n            <xs:enumeration value=\"Persisted\" />\r\n            <xs:enumeration value=\"Unloaded\" />\r\n            <xs:enumeration value=\"Loaded\" />\r\n            <xs:enumeration value=\"Exception\" />\r\n            <xs:enumeration value=\"Terminated\" />\r\n            <xs:enumeration value=\"Aborted\" />\r\n            <xs:enumeration value=\"Changed\" />\r\n            <xs:enumeration value=\"Started\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:complexType name=\"ExtractListType\">\r\n        <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n            <xs:element name=\"ActivityDataTrackingExtract\" type=\"ActivityDataTrackingExtractType\" />\r\n            <xs:element name=\"WorkflowDataTrackingExtract\" type=\"WorkflowDataTrackingExtractType\" />\r\n        </xs:choice>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"ActivityDataTrackingExtractType\">\r\n        <xs:sequence minOccurs=\"1\" maxOccurs=\"1\">\r\n            <xs:element name=\"Member\" type=\"xs:string\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:complexType name=\"WorkflowDataTrackingExtractType\">\r\n        <xs:sequence minOccurs=\"1\" maxOccurs=\"1\">\r\n            <xs:element name=\"Member\" type=\"xs:string\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n            <xs:element name=\"Annotations\" type=\"AnnotationListType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />\r\n        </xs:sequence>\r\n    </xs:complexType>\r\n\r\n    <xs:simpleType name=\"VersionType\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:pattern value=\"(^(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)|(^(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)|(^(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\\.(0*([0-9]\\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n\r\n    <xs:simpleType name=\"NonNullString\">\r\n        <xs:restriction base=\"xs:string\">\r\n            <xs:minLength value=\"1\" />\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n</xs:schema>";

        public TrackingProfileSerializer()
        {
            this._schema.Namespaces.Add("", "http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile");
        }

        private void CheckSchemaErrors()
        {
            if (this._vex)
            {
                TrackingProfileDeserializationException exception = new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationSchemaError);
                if (this._vArgs != null)
                {
                    foreach (ValidationEventArgs args in this._vArgs)
                    {
                        exception.ValidationEventArgs.Add(args);
                    }
                }
                throw exception;
            }
        }

        private void CreateActivityTrackingLocation(XmlReader reader, ActivityTrackingLocation location)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }
            if (string.Compare(reader.Name, "ActivityTrackingLocation", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "ActivityTrackingLocation.");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "ActivityTrackingLocation.");
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "TypeName", StringComparison.Ordinal) == 0)
                            {
                                if (null != location.ActivityType)
                                {
                                    throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidType);
                                }
                                location.ActivityTypeName = reader.ReadString();
                                break;
                            }
                            if (string.Compare(reader.Name, "Type", StringComparison.Ordinal) == 0)
                            {
                                if (location.ActivityTypeName != null)
                                {
                                    throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidType);
                                }
                                if (!reader.IsEmptyElement)
                                {
                                    string typeName = reader.ReadString();
                                    if ((typeName != null) && (typeName.Trim().Length > 0))
                                    {
                                        location.ActivityType = Type.GetType(typeName, true);
                                    }
                                }
                            }
                            else if (string.Compare(reader.Name, "MatchDerivedTypes", StringComparison.Ordinal) == 0)
                            {
                                location.MatchDerivedTypes = reader.ReadElementContentAsBoolean();
                            }
                            else if (string.Compare(reader.Name, "ExecutionStatusEvents", StringComparison.Ordinal) == 0)
                            {
                                this.CreateStatusEvents(reader, location.ExecutionStatusEvents);
                            }
                            else if (string.Compare(reader.Name, "Conditions", StringComparison.Ordinal) == 0)
                            {
                                this.CreateConditions(reader, location.Conditions);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "ActivityTrackingLocation", StringComparison.Ordinal) == 0)
                            {
                                if ((null == location.ActivityType) && (location.ActivityTypeName == null))
                                {
                                    location.ActivityType = typeof(Activity);
                                    location.MatchDerivedTypes = true;
                                }
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateActivityTrackingLocations(XmlReader reader, ActivityTrackingLocationCollection activities)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            if (!reader.IsEmptyElement)
            {
                string name = reader.Name;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "ActivityTrackingLocation", StringComparison.Ordinal) == 0)
                            {
                                ActivityTrackingLocation location = new ActivityTrackingLocation();
                                this.CreateActivityTrackingLocation(reader, location);
                                activities.Add(location);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            goto Label_006A;
                    }
                    continue;
                Label_006A:
                    if (string.Compare(name, reader.Name, StringComparison.Ordinal) == 0)
                    {
                        return;
                    }
                }
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + name + ".");
            }
        }

        private void CreateActivityTrackPoint(XmlReader reader, TrackingProfile profile)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (string.Compare(reader.Name, "ActivityTrackPoint", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "ActivityTrackPoint.");
            }
            if (!reader.IsEmptyElement)
            {
                ActivityTrackPoint item = new ActivityTrackPoint();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Annotations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateAnnotations(reader, item.Annotations);
                                break;
                            }
                            if (string.Compare(reader.Name, "MatchingLocations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateActivityTrackingLocations(reader, item.MatchingLocations);
                                break;
                            }
                            if (string.Compare(reader.Name, "ExcludedLocations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateActivityTrackingLocations(reader, item.ExcludedLocations);
                            }
                            else if (string.Compare(reader.Name, "Extracts", StringComparison.Ordinal) == 0)
                            {
                                this.CreateExtracts(reader, item.Extracts);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            goto Label_00FB;
                    }
                    continue;
                Label_00FB:
                    if (string.Compare(reader.Name, "ActivityTrackPoint", StringComparison.Ordinal) == 0)
                    {
                        profile.ActivityTrackPoints.Add(item);
                        return;
                    }
                }
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "ActivityTrackPoint.");
            }
        }

        private void CreateAnnotations(XmlReader reader, TrackingAnnotationCollection annotations)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (annotations == null)
            {
                throw new ArgumentNullException("annotations");
            }
            if (string.Compare(reader.Name, "Annotations", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "Annotations.");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "Annotations.");
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Annotation", StringComparison.Ordinal) == 0)
                            {
                                if (!reader.IsEmptyElement)
                                {
                                    annotations.Add(reader.ReadString());
                                }
                                else
                                {
                                    annotations.Add(null);
                                }
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "Annotations", StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateCondition(XmlReader reader, TrackingCondition condition)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }
            if (string.Compare(condition.GetType().Name, reader.Name, StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + condition.GetType().Name);
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + condition.GetType().Name);
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Member", StringComparison.Ordinal) == 0)
                            {
                                condition.Member = reader.ReadString();
                                break;
                            }
                            if (string.Compare(reader.Name, "Operator", StringComparison.Ordinal) == 0)
                            {
                                string strB = reader.ReadString();
                                if ((strB != null) && (strB.Trim().Length > 0))
                                {
                                    foreach (string str2 in Enum.GetNames(typeof(ComparisonOperator)))
                                    {
                                        if (string.Compare(str2, strB, StringComparison.Ordinal) == 0)
                                        {
                                            condition.Operator = (ComparisonOperator) Enum.Parse(typeof(ComparisonOperator), strB);
                                        }
                                    }
                                }
                            }
                            else if ((string.Compare(reader.Name, "Value", StringComparison.Ordinal) == 0) && !reader.IsEmptyElement)
                            {
                                condition.Value = reader.ReadString();
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, condition.GetType().Name, StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateConditions(XmlReader reader, TrackingConditionCollection conditions)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (conditions == null)
            {
                throw new ArgumentNullException("conditions");
            }
            if (string.Compare("Conditions", reader.Name, StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "Conditions.");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "Conditions.");
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "ActivityTrackingCondition", StringComparison.Ordinal) == 0)
                            {
                                ActivityTrackingCondition condition = new ActivityTrackingCondition();
                                this.CreateCondition(reader, condition);
                                conditions.Add(condition);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "Conditions", StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateExtract(XmlReader reader, TrackingExtract extract)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (extract == null)
            {
                throw new ArgumentNullException("extract");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + extract.GetType().Name);
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Member", StringComparison.Ordinal) == 0)
                            {
                                extract.Member = reader.ReadString();
                            }
                            else if (string.Compare(reader.Name, "Annotations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateAnnotations(reader, extract.Annotations);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, extract.GetType().Name, StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateExtracts(XmlReader reader, ExtractCollection extracts)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (extracts == null)
            {
                throw new ArgumentNullException("extracts");
            }
            if (string.Compare("Extracts", reader.Name, StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "Extracts");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "Extracts.");
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "ActivityDataTrackingExtract", StringComparison.Ordinal) == 0)
                            {
                                ActivityDataTrackingExtract extract = new ActivityDataTrackingExtract();
                                this.CreateExtract(reader, extract);
                                extracts.Add(extract);
                            }
                            else if (string.Compare(reader.Name, "WorkflowDataTrackingExtract", StringComparison.Ordinal) == 0)
                            {
                                WorkflowDataTrackingExtract extract2 = new WorkflowDataTrackingExtract();
                                this.CreateExtract(reader, extract2);
                                extracts.Add(extract2);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "Extracts", StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateStatusEvents(XmlReader reader, IList<ActivityExecutionStatus> events)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (events == null)
            {
                throw new ArgumentNullException("events");
            }
            if (string.Compare("ExecutionStatusEvents", reader.Name, StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "ExecutionStatusEvents.");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "ExecutionStatusEvents.");
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "ExecutionStatus", StringComparison.Ordinal) == 0)
                            {
                                string strB = reader.ReadString();
                                if ((strB != null) && (strB.Trim().Length > 0))
                                {
                                    foreach (string str2 in Enum.GetNames(typeof(ActivityExecutionStatus)))
                                    {
                                        if (string.Compare(str2, strB, StringComparison.Ordinal) == 0)
                                        {
                                            events.Add((ActivityExecutionStatus) Enum.Parse(typeof(ActivityExecutionStatus), strB));
                                        }
                                    }
                                }
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "ExecutionStatusEvents", StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateTrackPoints(XmlReader reader, TrackingProfile profile)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (string.Compare(reader.Name, "TrackPoints", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "TrackPoints.");
            }
            if (!reader.IsEmptyElement)
            {
                while (true)
                {
                    if (!reader.Read())
                    {
                        throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "TrackPoints.");
                    }
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "ActivityTrackPoint", StringComparison.Ordinal) == 0)
                            {
                                this.CreateActivityTrackPoint(reader, profile);
                                break;
                            }
                            if (string.Compare(reader.Name, "UserTrackPoint", StringComparison.Ordinal) == 0)
                            {
                                this.CreateUserTrackPoint(reader, profile);
                            }
                            else if (string.Compare(reader.Name, "WorkflowTrackPoint", StringComparison.Ordinal) == 0)
                            {
                                this.CreateWorkflowTrackPoint(reader, profile);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "TrackPoints", StringComparison.Ordinal) == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void CreateUserTrackingLocation(XmlReader reader, UserTrackingLocation location)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }
            if (string.Compare(reader.Name, "UserTrackingLocation", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "UserTrackingLocation.");
            }
            if (!reader.IsEmptyElement)
            {
                string str = null;
                string typeName = null;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Activity", StringComparison.Ordinal) == 0)
                            {
                                flag2 = true;
                                break;
                            }
                            if (string.Compare(reader.Name, "KeyName", StringComparison.Ordinal) == 0)
                            {
                                location.KeyName = reader.ReadString();
                                break;
                            }
                            if (string.Compare(reader.Name, "Argument", StringComparison.Ordinal) == 0)
                            {
                                flag3 = true;
                                break;
                            }
                            if (string.Compare(reader.Name, "TypeName", StringComparison.Ordinal) == 0)
                            {
                                str = reader.ReadString();
                                break;
                            }
                            if (string.Compare(reader.Name, "Type", StringComparison.Ordinal) == 0)
                            {
                                typeName = reader.ReadString();
                                break;
                            }
                            if (string.Compare(reader.Name, "MatchDerivedTypes", StringComparison.Ordinal) == 0)
                            {
                                flag = reader.ReadElementContentAsBoolean();
                            }
                            else if (string.Compare(reader.Name, "Conditions", StringComparison.Ordinal) == 0)
                            {
                                this.CreateConditions(reader, location.Conditions);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (string.Compare(reader.Name, "UserTrackingLocation", StringComparison.Ordinal) != 0)
                            {
                                goto Label_01FD;
                            }
                            if (!flag2)
                            {
                                location.ActivityType = typeof(Activity);
                                location.MatchDerivedActivityTypes = true;
                            }
                            if (!flag3)
                            {
                                location.ArgumentType = typeof(object);
                                location.MatchDerivedArgumentTypes = true;
                            }
                            if (((null == location.ActivityType) && ((location.ActivityTypeName == null) || (location.ActivityTypeName.Trim().Length == 0))) && ((null == location.ArgumentType) && ((location.ArgumentTypeName == null) || (location.ArgumentTypeName.Trim().Length == 0))))
                            {
                                throw new TrackingProfileDeserializationException(ExecutionStringManager.MissingActivityType);
                            }
                            return;
                    }
                    continue;
                Label_01FD:
                    if (string.Compare(reader.Name, "Activity", StringComparison.Ordinal) == 0)
                    {
                        if (!flag2)
                        {
                            location.ActivityType = typeof(Activity);
                            location.MatchDerivedActivityTypes = true;
                        }
                        else
                        {
                            if ((typeName != null) && (typeName.Trim().Length > 0))
                            {
                                location.ActivityType = Type.GetType(typeName, true);
                            }
                            else
                            {
                                location.ActivityTypeName = str;
                            }
                            location.MatchDerivedActivityTypes = flag;
                        }
                        str = null;
                        typeName = null;
                        flag = false;
                    }
                    else if (string.Compare(reader.Name, "Argument", StringComparison.Ordinal) == 0)
                    {
                        if (!flag3)
                        {
                            location.ArgumentType = typeof(object);
                            location.MatchDerivedArgumentTypes = true;
                        }
                        else
                        {
                            if ((typeName != null) && (typeName.Trim().Length > 0))
                            {
                                location.ArgumentType = Type.GetType(typeName, true);
                            }
                            else
                            {
                                location.ArgumentTypeName = str;
                            }
                            location.MatchDerivedArgumentTypes = flag;
                        }
                        str = null;
                        typeName = null;
                        flag = false;
                    }
                }
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "UserTrackingLocation.");
            }
        }

        private void CreateUserTrackingLocations(XmlReader reader, UserTrackingLocationCollection user)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (!reader.IsEmptyElement)
            {
                string name = reader.Name;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "UserTrackingLocation", StringComparison.Ordinal) == 0)
                            {
                                UserTrackingLocation location = new UserTrackingLocation();
                                this.CreateUserTrackingLocation(reader, location);
                                user.Add(location);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            goto Label_006A;
                    }
                    continue;
                Label_006A:
                    if (string.Compare(name, reader.Name, StringComparison.Ordinal) == 0)
                    {
                        return;
                    }
                }
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + name + ".");
            }
        }

        private void CreateUserTrackPoint(XmlReader reader, TrackingProfile profile)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (string.Compare(reader.Name, "UserTrackPoint", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "UserTrackPoint.");
            }
            if (!reader.IsEmptyElement)
            {
                UserTrackPoint item = new UserTrackPoint();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Annotations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateAnnotations(reader, item.Annotations);
                                break;
                            }
                            if (string.Compare(reader.Name, "MatchingLocations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateUserTrackingLocations(reader, item.MatchingLocations);
                                break;
                            }
                            if (string.Compare(reader.Name, "ExcludedLocations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateUserTrackingLocations(reader, item.ExcludedLocations);
                            }
                            else if (string.Compare(reader.Name, "Extracts", StringComparison.Ordinal) == 0)
                            {
                                this.CreateExtracts(reader, item.Extracts);
                            }
                            break;

                        case XmlNodeType.EndElement:
                            goto Label_00FB;
                    }
                    continue;
                Label_00FB:
                    if (string.Compare(reader.Name, "UserTrackPoint", StringComparison.Ordinal) == 0)
                    {
                        profile.UserTrackPoints.Add(item);
                        return;
                    }
                }
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "UserTrackPoint.");
            }
        }

        private void CreateWorkflowTrackPoint(XmlReader reader, TrackingProfile profile)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (string.Compare(reader.Name, "WorkflowTrackPoint", StringComparison.Ordinal) != 0)
            {
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "WorkflowTrackPoint.");
            }
            if (!reader.IsEmptyElement)
            {
                WorkflowTrackPoint item = new WorkflowTrackPoint {
                    MatchingLocation = new WorkflowTrackingLocation()
                };
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Compare(reader.Name, "Annotations", StringComparison.Ordinal) == 0)
                            {
                                this.CreateAnnotations(reader, item.Annotations);
                            }
                            else if (string.Compare(reader.Name, "TrackingWorkflowEvent", StringComparison.Ordinal) == 0)
                            {
                                item.MatchingLocation.Events.Add((TrackingWorkflowEvent) Enum.Parse(typeof(TrackingWorkflowEvent), reader.ReadString()));
                            }
                            break;

                        case XmlNodeType.EndElement:
                            goto Label_00D9;
                    }
                    continue;
                Label_00D9:
                    if (string.Compare(reader.Name, "WorkflowTrackPoint", StringComparison.Ordinal) == 0)
                    {
                        profile.WorkflowTrackPoints.Add(item);
                        return;
                    }
                }
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "WorkflowTrackPoint.");
            }
        }

        public TrackingProfile Deserialize(TextReader reader)
        {
            TrackingProfile profile = null;
            this._vArgs = new List<ValidationEventArgs>();
            this._vex = false;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            NameTable nameTable = new NameTable();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(nameTable);
            nsMgr.AddNamespace(string.Empty, "http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile");
            XmlParserContext inputContext = new XmlParserContext(nameTable, nsMgr, null, XmlSpace.None);
            XmlReader reader2 = XmlReader.Create(reader, this.GetSchemaReaderSettings(), inputContext);
            try
            {
                profile = new TrackingProfile();
                if (!reader2.ReadToDescendant("TrackingProfile"))
                {
                    this.CheckSchemaErrors();
                    return null;
                }
                string attribute = reader2.GetAttribute("version");
                if ((attribute == null) || (attribute.Trim().Length == 0))
                {
                    throw new TrackingProfileDeserializationException(ExecutionStringManager.InvalidProfileVersion);
                }
                profile.Version = new Version(attribute);
                if (!reader2.ReadToDescendant("TrackPoints"))
                {
                    this.CheckSchemaErrors();
                    return null;
                }
                this.CreateTrackPoints(reader2, profile);
                this.CheckSchemaErrors();
            }
            catch (Exception)
            {
                profile = null;
                throw;
            }
            finally
            {
                this._vArgs = new List<ValidationEventArgs>();
                this._vex = false;
                reader2.Close();
            }
            return profile;
        }

        private XmlReaderSettings GetSchemaReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(this._schema);
            settings.ValidationType = ValidationType.Schema;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.CloseInput = false;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            return settings;
        }

        private void InitWriter(XmlTextWriter writer)
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
        }

        private bool IsStatus(int val)
        {
            foreach (ActivityExecutionStatus status in Enum.GetValues(typeof(ActivityExecutionStatus)))
            {
                if (((int) status) == val)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWorkflowEvent(int val)
        {
            foreach (TrackingWorkflowEvent event2 in Enum.GetValues(typeof(TrackingWorkflowEvent)))
            {
                if (event2 == val)
                {
                    return true;
                }
            }
            return false;
        }

        public void Serialize(TextWriter writer, TrackingProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            XmlTextWriter writer2 = new XmlTextWriter(writer);
            this.InitWriter(writer2);
            this.Write(profile, writer2);
            writer2.Flush();
            writer2.Close();
        }

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            this._vArgs.Add(e);
            if (e.Severity == XmlSeverityType.Error)
            {
                this._vex = true;
            }
        }

        private void Write(TrackingProfile profile, XmlTextWriter writer)
        {
            writer.WriteStartDocument(true);
            writer.WriteStartElement("TrackingProfile");
            writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile");
            if (null == profile.Version)
            {
                throw new ArgumentException(ExecutionStringManager.InvalidProfileVersion);
            }
            string str = null;
            if (profile.Version.Revision >= 0)
            {
                str = string.Format(NumberFormatInfo.InvariantInfo, "{0}.{1}.{2}.{3}", new object[] { profile.Version.Major, profile.Version.Minor, profile.Version.Build, profile.Version.Revision });
            }
            else if (profile.Version.Build >= 0)
            {
                str = string.Format(NumberFormatInfo.InvariantInfo, "{0}.{1}.{2}", new object[] { profile.Version.Major, profile.Version.Minor, profile.Version.Build });
            }
            else if (profile.Version.Minor >= 0)
            {
                str = string.Format(NumberFormatInfo.InvariantInfo, "{0}.{1}", new object[] { profile.Version.Major, profile.Version.Minor });
            }
            writer.WriteAttributeString("version", str);
            this.WriteTrackPoints(profile, writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private void WriteActivityTrackingLocation(ActivityTrackingLocation loc, XmlTextWriter writer)
        {
            if ((null == loc.ActivityType) && ((loc.ActivityTypeName == null) || (loc.ActivityTypeName.Trim().Length == 0)))
            {
                throw new ArgumentException(ExecutionStringManager.MissingActivityType);
            }
            writer.WriteStartElement("ActivityTrackingLocation");
            writer.WriteStartElement("Activity");
            if (null != loc.ActivityType)
            {
                writer.WriteElementString("Type", loc.ActivityType.AssemblyQualifiedName);
            }
            else
            {
                writer.WriteElementString("TypeName", loc.ActivityTypeName);
            }
            writer.WriteElementString("MatchDerivedTypes", loc.MatchDerivedTypes.ToString().ToLower(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            this.WriteEvents(loc.ExecutionStatusEvents, writer);
            if ((loc.Conditions != null) && (loc.Conditions.Count > 0))
            {
                this.WriteConditions(loc.Conditions, writer);
            }
            writer.WriteEndElement();
        }

        private void WriteActivityTrackPoint(ActivityTrackPoint point, XmlTextWriter writer)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point");
            }
            if ((point.MatchingLocations == null) || (point.MatchingLocations.Count == 0))
            {
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);
            }
            writer.WriteStartElement("ActivityTrackPoint");
            writer.WriteStartElement("MatchingLocations");
            int num = 0;
            foreach (ActivityTrackingLocation location in point.MatchingLocations)
            {
                if (location != null)
                {
                    this.WriteActivityTrackingLocation(location, writer);
                    num++;
                }
            }
            if (num == 0)
            {
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);
            }
            writer.WriteEndElement();
            if ((point.ExcludedLocations != null) && (point.ExcludedLocations.Count > 0))
            {
                writer.WriteStartElement("ExcludedLocations");
                foreach (ActivityTrackingLocation location2 in point.ExcludedLocations)
                {
                    if (location2 != null)
                    {
                        this.WriteActivityTrackingLocation(location2, writer);
                    }
                }
                writer.WriteEndElement();
            }
            this.WriteAnnotations(point.Annotations, writer);
            this.WriteExtracts(point.Extracts, writer);
            writer.WriteEndElement();
        }

        private void WriteAnnotations(TrackingAnnotationCollection annotations, XmlTextWriter writer)
        {
            if ((annotations != null) && (annotations.Count != 0))
            {
                writer.WriteStartElement("Annotations");
                foreach (string str in annotations)
                {
                    writer.WriteStartElement("Annotation");
                    if ((str == null) || (str.Length > 0))
                    {
                        writer.WriteValue((str == null) ? string.Empty : str);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteFullEndElement();
                    }
                }
                writer.WriteEndElement();
            }
        }

        private void WriteCondition(TrackingCondition condition, XmlTextWriter writer)
        {
            if (condition != null)
            {
                writer.WriteStartElement(condition.GetType().Name);
                writer.WriteElementString("Operator", condition.Operator.ToString());
                if ((condition.Member == null) || (condition.Member.Trim().Length == 0))
                {
                    throw new ArgumentException(ExecutionStringManager.MissingMemberName);
                }
                writer.WriteElementString("Member", condition.Member);
                if (condition.Value != null)
                {
                    if (string.Empty == condition.Value)
                    {
                        writer.WriteStartElement("Value");
                        writer.WriteRaw(string.Empty);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteElementString("Value", condition.Value);
                    }
                }
                writer.WriteEndElement();
            }
        }

        private void WriteConditions(TrackingConditionCollection conditions, XmlTextWriter writer)
        {
            if ((conditions != null) && (conditions.Count != 0))
            {
                writer.WriteStartElement("Conditions");
                foreach (TrackingCondition condition in conditions)
                {
                    if (condition != null)
                    {
                        this.WriteCondition(condition, writer);
                    }
                }
                writer.WriteEndElement();
            }
        }

        private void WriteEvents(IList<ActivityExecutionStatus> events, XmlTextWriter writer)
        {
            if ((events == null) || (events.Count == 0))
            {
                throw new ArgumentException(ExecutionStringManager.MissingActivityEvents);
            }
            writer.WriteStartElement("ExecutionStatusEvents");
            foreach (ActivityExecutionStatus status in events)
            {
                if (!this.IsStatus((int) status))
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidStatus);
                }
                writer.WriteStartElement("ExecutionStatus");
                writer.WriteValue(status.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteExtract(TrackingExtract extract, XmlTextWriter writer)
        {
            extract.GetType();
            if (!(extract is ActivityDataTrackingExtract) && !(extract is WorkflowDataTrackingExtract))
            {
                throw new ArgumentException(ExecutionStringManager.TrackingSerializationInvalidExtract);
            }
            writer.WriteStartElement(extract.GetType().Name);
            writer.WriteElementString("Member", (extract.Member == null) ? string.Empty : extract.Member);
            this.WriteAnnotations(extract.Annotations, writer);
            writer.WriteEndElement();
        }

        private void WriteExtracts(ExtractCollection extracts, XmlTextWriter writer)
        {
            if ((extracts != null) && (extracts.Count != 0))
            {
                writer.WriteStartElement("Extracts");
                foreach (TrackingExtract extract in extracts)
                {
                    if (extract != null)
                    {
                        this.WriteExtract(extract, writer);
                    }
                }
                writer.WriteEndElement();
            }
        }

        private void WriteTrackPoints(TrackingProfile profile, XmlTextWriter writer)
        {
            if ((((profile.WorkflowTrackPoints == null) || (profile.WorkflowTrackPoints.Count == 0)) && ((profile.ActivityTrackPoints == null) || (profile.ActivityTrackPoints.Count == 0))) && ((profile.UserTrackPoints == null) || (profile.UserTrackPoints.Count == 0)))
            {
                throw new ArgumentException(ExecutionStringManager.TrackingSerializationNoTrackPoints);
            }
            int num = 0;
            writer.WriteStartElement("TrackPoints");
            foreach (WorkflowTrackPoint point in profile.WorkflowTrackPoints)
            {
                if (point != null)
                {
                    this.WriteWorkflowTrackPoint(point, writer);
                    num++;
                }
            }
            foreach (ActivityTrackPoint point2 in profile.ActivityTrackPoints)
            {
                if (point2 != null)
                {
                    this.WriteActivityTrackPoint(point2, writer);
                    num++;
                }
            }
            foreach (UserTrackPoint point3 in profile.UserTrackPoints)
            {
                if (point3 != null)
                {
                    this.WriteUserTrackPoint(point3, writer);
                    num++;
                }
            }
            if (num == 0)
            {
                throw new ArgumentException(ExecutionStringManager.TrackingSerializationNoTrackPoints);
            }
            writer.WriteEndElement();
        }

        private void WriteUserTrackingLocation(UserTrackingLocation loc, XmlTextWriter writer)
        {
            if ((null == loc.ActivityType) && ((loc.ActivityTypeName == null) || (loc.ActivityTypeName.Trim().Length == 0)))
            {
                throw new ArgumentException(ExecutionStringManager.MissingActivityType);
            }
            if ((null == loc.ArgumentType) && ((loc.ArgumentTypeName == null) || (loc.ArgumentTypeName.Trim().Length == 0)))
            {
                throw new ArgumentException(ExecutionStringManager.MissingArgumentType);
            }
            writer.WriteStartElement("UserTrackingLocation");
            writer.WriteStartElement("Activity");
            if (null != loc.ActivityType)
            {
                writer.WriteElementString("Type", loc.ActivityType.AssemblyQualifiedName);
            }
            else
            {
                writer.WriteElementString("TypeName", loc.ActivityTypeName);
            }
            writer.WriteElementString("MatchDerivedTypes", loc.MatchDerivedActivityTypes.ToString().ToLower(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            if (loc.KeyName != null)
            {
                writer.WriteElementString("KeyName", loc.KeyName);
            }
            writer.WriteStartElement("Argument");
            if (null != loc.ArgumentType)
            {
                writer.WriteElementString("Type", loc.ArgumentType.AssemblyQualifiedName);
            }
            else
            {
                writer.WriteElementString("TypeName", loc.ArgumentTypeName);
            }
            writer.WriteElementString("MatchDerivedTypes", loc.MatchDerivedArgumentTypes.ToString().ToLower(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            if ((loc.Conditions != null) && (loc.Conditions.Count > 0))
            {
                this.WriteConditions(loc.Conditions, writer);
            }
            writer.WriteEndElement();
        }

        private void WriteUserTrackPoint(UserTrackPoint point, XmlTextWriter writer)
        {
            if ((point.MatchingLocations == null) || (point.MatchingLocations.Count == 0))
            {
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);
            }
            writer.WriteStartElement("UserTrackPoint");
            writer.WriteStartElement("MatchingLocations");
            int num = 0;
            foreach (UserTrackingLocation location in point.MatchingLocations)
            {
                if (location != null)
                {
                    this.WriteUserTrackingLocation(location, writer);
                    num++;
                }
            }
            if (num == 0)
            {
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);
            }
            writer.WriteEndElement();
            if ((point.ExcludedLocations != null) && (point.ExcludedLocations.Count > 0))
            {
                writer.WriteStartElement("ExcludedLocations");
                foreach (UserTrackingLocation location2 in point.ExcludedLocations)
                {
                    if (location2 != null)
                    {
                        this.WriteUserTrackingLocation(location2, writer);
                    }
                }
                writer.WriteEndElement();
            }
            this.WriteAnnotations(point.Annotations, writer);
            this.WriteExtracts(point.Extracts, writer);
            writer.WriteEndElement();
        }

        private void WriteWorkflowEvents(IList<TrackingWorkflowEvent> events, XmlTextWriter writer)
        {
            if ((events != null) && (events.Count != 0))
            {
                writer.WriteStartElement("TrackingWorkflowEvents");
                foreach (TrackingWorkflowEvent event2 in events)
                {
                    if (!this.IsWorkflowEvent((int) event2))
                    {
                        throw new ArgumentException(ExecutionStringManager.InvalidWorkflowEvent);
                    }
                    writer.WriteStartElement("TrackingWorkflowEvent");
                    writer.WriteValue(event2.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private void WriteWorkflowTrackingLocation(WorkflowTrackingLocation loc, XmlTextWriter writer)
        {
            if ((loc.Events == null) || (loc.Events.Count == 0))
            {
                throw new ArgumentException(ExecutionStringManager.MissingWorkflowEvents);
            }
            writer.WriteStartElement("MatchingLocation");
            writer.WriteStartElement("WorkflowTrackingLocation");
            this.WriteWorkflowEvents(loc.Events, writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteWorkflowTrackPoint(WorkflowTrackPoint point, XmlTextWriter writer)
        {
            if (point.MatchingLocation == null)
            {
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocation);
            }
            writer.WriteStartElement("WorkflowTrackPoint");
            this.WriteWorkflowTrackingLocation(point.MatchingLocation, writer);
            this.WriteAnnotations(point.Annotations, writer);
            writer.WriteEndElement();
        }

        public XmlSchema Schema
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._schema;
            }
        }
    }
}

