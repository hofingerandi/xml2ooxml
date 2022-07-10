using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml2ooxml.Tests
{
    internal class SampleData
    {
        public const string VisuTest = """
<?xml version="1.0" encoding="utf-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0200">
	<fileHeader companyName=""/>
	<contentHeader name="VisuTest.project">
		<coordinateInfo>
			<fbd>
				<scaling x="1" y="1"/>
			</fbd>
			<ld>
				<scaling x="1" y="1"/>
			</ld>
			<sfc>
				<scaling x="1" y="1"/>
			</sfc>
		</coordinateInfo>
		<addData>
			<data name="http://www.3s-software.com/plcopenxml/projectinformation" handleUnknown="implementation">
				<ProjectInformation/>
			</data>
		</addData>
	</contentHeader>
	<types>
		<dataTypes/>
		<pous/>
	</types>
	<instances>
		<configurations>
			<configuration name="Device">
				<resource name="Application">
					<task name="Task" interval="PT0.02S" priority="1">
						<addData></addData>
					</task>
					<addData>
						<data name="http://www.3s-software.com/plcopenxml/datatype" handleUnknown="implementation">
							<dataType name="DUT"></dataType>
						</data>
						<data name="http://www.3s-software.com/plcopenxml/pou" handleUnknown="implementation">
							<pou name="PLC_PRG" pouType="program">
								<interface iType="explicit">
									<localVars>
										<variable name="ResetTrigger">
											<type>
												<derived name="R_TRIG"/>
											</type>
										</variable>
									</localVars>
								</interface>
								<body type="methodBody">
									<ST lang="structText">
										<xhtml xmlns="http://www.w3.org/1999/xhtml">ButtonTrigger(CLK:= ButtonPressed, Q=&gt; );
IF (ButtonTrigger.Q) THEN
	Counter := Counter + 1;
END_IF

ResetTrigger(CLK:= ResetPressed, Q=&gt; );
IF (ResetTrigger.Q) THEN
	Counter := 0;
END_IF</xhtml>
									</ST>
								</body>
							</pou>
						</data>
					</addData>
				</resource>
			</configuration>
		</configurations>
	</instances>
</project>
""";
    }
}
