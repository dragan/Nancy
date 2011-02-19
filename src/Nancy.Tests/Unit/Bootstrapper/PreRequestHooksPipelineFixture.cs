namespace Pipelines
{
    using System;
    using System.Linq;
    using Nancy;
    using Nancy.Bootstrapper;
    using Xunit;

    public class PreRequestHooksPipelineTests
    {
        private PreRequestHooksPipeline pipeline;

        private static Response CreateResponse()
        {
            return new Response();
        }

        private static NancyContext CreateContext()
        {
            return new NancyContext();
        }

        public PreRequestHooksPipelineTests()
        {
            this.pipeline = new PreRequestHooksPipeline();
        }

        [Fact]
        public void When_invoked_pipeline_member_returning_a_response_stops_pipeline_execution()
        {
            var item1Called = false;
            Func<NancyContext, Response> item1 = (r) => { item1Called = true; return null; };
            var item2Called = false;
            Func<NancyContext, Response> item2 = (r) => { item2Called = true; return CreateResponse(); };
            var item3Called = false;
            Func<NancyContext, Response> item3 = (r) => { item3Called = true; return null; };
            pipeline.AddItemToEndOfPipeline(item1);
            pipeline.AddItemToEndOfPipeline(item2);
            pipeline.AddItemToEndOfPipeline(item3);

            pipeline.Invoke(CreateContext());

            Assert.True(item1Called);
            Assert.True(item2Called);
            Assert.False(item3Called);
        }

        [Fact]
        public void When_invoked_pipeline_member_returning_a_response_returns_that_response()
        {
            var response = CreateResponse();
            Func<NancyContext, Response> item1 = (r) => null;
            Func<NancyContext, Response> item2 = (r) => response;
            Func<NancyContext, Response> item3 = (r) => null;
            pipeline.AddItemToEndOfPipeline(item1);
            pipeline.AddItemToEndOfPipeline(item2);
            pipeline.AddItemToEndOfPipeline(item3);

            var result = pipeline.Invoke(CreateContext());

            Assert.Same(response, result);
        }

        [Fact]
        public void When_invoked_pipeline_members_all_return_null_returns_null()
        {
            Func<NancyContext, Response> item1 = (r) => null;
            Func<NancyContext, Response> item2 = (r) => null;
            Func<NancyContext, Response> item3 = (r) => null;
            pipeline.AddItemToEndOfPipeline(item1);
            pipeline.AddItemToEndOfPipeline(item2);
            pipeline.AddItemToEndOfPipeline(item3);

            var result = pipeline.Invoke(CreateContext());

            Assert.Null(result);
        }

        [Fact]
        public void AddItemToEndOfPipeline_adds_to_the_end_of_the_pipeline()
        {
            Func<NancyContext, Response> item1 = (r) => { return null; };
            Func<NancyContext, Response> item2 = (r) => { return CreateResponse(); };
            pipeline.AddItemToEndOfPipeline(item2);

            pipeline.AddItemToEndOfPipeline(item1);

            Assert.Equal(2, pipeline.PipelineItems.Count());
            Assert.Same(item1, pipeline.PipelineItems.Last());
        }

        [Fact]
        public void AddItemToStartOfPipeline_adds_to_the_end_of_the_pipeline()
        {
            Func<NancyContext, Response> item1 = (r) => { return null; };
            Func<NancyContext, Response> item2 = (r) => { return new Response(); };
            pipeline.AddItemToEndOfPipeline(item2);

            pipeline.AddItemToStartOfPipeline(item1);

            Assert.Equal(2, pipeline.PipelineItems.Count());
            Assert.Same(item1, pipeline.PipelineItems.First());
        }

        [Fact]
        public void InsertItemAtPipelineIndex_adds_at_correct_index()
        {
            Func<NancyContext, Response> item1 = (r) => null;
            Func<NancyContext, Response> item2 = (r) => null;
            Func<NancyContext, Response> item3 = (r) => null;
            pipeline.AddItemToEndOfPipeline(item1);
            pipeline.AddItemToEndOfPipeline(item3);

            pipeline.InsertItemAtPipelineIndex(1, item2);

            Assert.Same(item1, pipeline.PipelineItems.ElementAt(0));
            Assert.Same(item2, pipeline.PipelineItems.ElementAt(1));
            Assert.Same(item3, pipeline.PipelineItems.ElementAt(2));
        }

        [Fact]
        public void PlusEquals_with_func_add_item_to_end_of_pipeline()
        {
            Func<NancyContext, Response> item1 = (r) => { return null; };
            Func<NancyContext, Response> item2 = (r) => { return CreateResponse(); };
            pipeline.AddItemToEndOfPipeline(item2);

            pipeline += item1;

            Assert.Equal(2, pipeline.PipelineItems.Count());
            Assert.Same(item1, pipeline.PipelineItems.Last());
        }

        [Fact]
        public void PlusEquals_with_another_pipeline_adds_those_pipeline_items_to_end_of_pipeline()
        {
            Func<NancyContext, Response> item1 = (r) => { return null; };
            Func<NancyContext, Response> item2 = (r) => { return CreateResponse(); };
            pipeline.AddItemToEndOfPipeline(item1);
            pipeline.AddItemToEndOfPipeline(item2);
            Func<NancyContext, Response> item3 = (r) => { return null; };
            Func<NancyContext, Response> item4 = (r) => { return CreateResponse(); };
            var pipeline2 = new PreRequestHooksPipeline();
            pipeline2.AddItemToEndOfPipeline(item3);
            pipeline2.AddItemToEndOfPipeline(item4);

            pipeline += pipeline2;

            Assert.Equal(4, pipeline.PipelineItems.Count());
            Assert.Same(item3, pipeline.PipelineItems.ElementAt(2));
            Assert.Same(item4, pipeline.PipelineItems.Last());
        }

        [Fact]
        public void When_cast_to_func_and_invoked_members_are_invoked()
        {
            var item1Called = false;
            Func<NancyContext, Response> item1 = (r) => { item1Called = true; return null; };
            var item2Called = false;
            Func<NancyContext, Response> item2 = (r) => { item2Called = true; return null; };
            var item3Called = false;
            Func<NancyContext, Response> item3 = (r) => { item3Called = true; return null; };
            pipeline.AddItemToEndOfPipeline(item1);
            pipeline.AddItemToEndOfPipeline(item2);
            pipeline.AddItemToEndOfPipeline(item3);

            Func<NancyContext, Response> func = pipeline;
            func.Invoke(CreateContext());

            Assert.True(item1Called);
            Assert.True(item2Called);
            Assert.True(item3Called);
        }

        [Fact]
        public void When_cast_from_func_creates_a_pipeline_with_one_item()
        {
            Func<NancyContext, Response> item1 = (r) => null;

            PreRequestHooksPipeline castPipeline = item1;

            Assert.Equal(1, castPipeline.PipelineItems.Count());
            Assert.Same(item1, castPipeline.PipelineItems.First());
        }

        [Fact]
        public void Pipeline_containing_another_pipeline_will_invoke_items_in_both_pipelines()
        {
            var item1Called = false;
            Func<NancyContext, Response> item1 = (r) => { item1Called = true; return null; };
            var item2Called = false;
            Func<NancyContext, Response> item2 = (r) => { item2Called = true; return null; };
            var item3Called = false;
            Func<NancyContext, Response> item3 = (r) => { item3Called = true; return null; };
            var item4Called = false;
            Func<NancyContext, Response> item4 = (r) => { item4Called = true; return null; };
            pipeline += item1;
            pipeline += item2;
            var subPipeline = new PreRequestHooksPipeline();
            subPipeline += item3;
            subPipeline += item4;

            pipeline.AddItemToEndOfPipeline(subPipeline);
            pipeline.Invoke(CreateContext());

            Assert.True(item1Called);
            Assert.True(item2Called);
            Assert.True(item3Called);
            Assert.True(item4Called);
        }
    }
}